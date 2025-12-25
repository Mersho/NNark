#!/usr/bin/env bash
set -e

# Navigate to repository root first
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

log() {
  local msg="$1"
  local green="\033[0;32m"
  local reset="\033[0m"
  echo -e "${green}[$(date '+%H:%M:%S')] ${msg}${reset}"
}

wait_for_service() {
  local url="$1"
  local service_name="$2"
  local max_attempts="${3:-30}"
  
  log "Waiting for $service_name to be ready..."
  local attempt=1
  while [ $attempt -le $max_attempts ]; do
    if curl -s "$url" >/dev/null 2>&1; then
      log "$service_name is ready!"
      return 0
    fi
    log "Waiting for $service_name... (attempt $attempt/$max_attempts)"
    sleep 2
    ((attempt++))
  done
  
  log "ERROR: $service_name failed to start within expected time"
  return 1
}

mine_blocks() {
  local count="${1:-1}"
  log "Mining $count blocks..."
  curl -s -X POST http://localhost:3000/faucet -H "Content-Type: application/json" -d "{\"address\":\"bcrt1qxhmdufsvnuaaaer4ynz88fspdsxq2h9e9cetdj\",\"amount\":0}" >/dev/null
  for ((i=1; i<=$count; i++)); do
    curl -s -X POST http://localhost:3000/mining >/dev/null
  done
}

faucet() {
  local address="$1"
  local amount="${2:-1}"
  log "Funding address $address with $amount BTC..."
  curl -s -X POST http://localhost:3000/faucet \
    -H "Content-Type: application/json" \
    -d "{\"address\":\"$address\",\"amount\":$amount}"
  mine_blocks 1
}

setup_lnd_wallet() {
  log "Setting up LND wallets for Lightning swaps..."
  
  wait_for_service "http://localhost:10010" "Boltz LND" 60 || exit 1
  wait_for_service "http://localhost:10009" "User LND" 60 || exit 1
  
  sleep 5

  # Fund boltz-lnd wallet
  log "Getting Boltz LND address..."
  ln_address=$(docker exec boltz-lnd lncli --network=regtest newaddress p2wkh | jq -r '.address')
  log "Boltz LND address: $ln_address"

  faucet "$ln_address" 2
  sleep 5

  lnd_balance=$(docker exec boltz-lnd lncli --network=regtest walletbalance | jq -r '.confirmed_balance // .account_balance.default.confirmed_balance // "0"')
  log "Boltz LND balance: $lnd_balance sats"

  if [ "$lnd_balance" -lt 100000 ]; then
    log "WARNING: Boltz LND wallet balance ($lnd_balance) is low"
  fi

  # Open channel to user LND node
  counterparty_node_pubkey=$(docker exec lnd lncli --network=regtest getinfo | jq -r '.identity_pubkey')
  log "Opening channel to user LND node ($counterparty_node_pubkey)..."
  docker exec boltz-lnd lncli --network=regtest openchannel \
    --node_key "$counterparty_node_pubkey" \
    --connect "lnd:9735" \
    --local_amt 1000000 \
    --sat_per_vbyte 1 \
    --min_confs 0 || log "Channel opening may have failed or already exists"

  log "Mining blocks to confirm channel..."
  mine_blocks 10
  sleep 10

  # Test channel with small payment
  log "Testing channel with small payment..."
  invoice=$(docker exec lnd lncli --network=regtest addinvoice --amt 500000 | jq -r '.payment_request')
  docker exec boltz-lnd lncli --network=regtest payinvoice --force $invoice || log "Test payment failed, channel may not be active yet"

  log "✓ LND wallets setup completed!"
}

setup_fulmine_wallet() {
  log "Setting up Fulmine wallet..."
  
  wait_for_service "http://localhost:7003/api/v1/wallet/status" "Fulmine" 30 || exit 1

  # Generate seed
  log "Generating seed..."
  seed_response=$(curl -s -X GET http://localhost:7003/api/v1/wallet/genseed)
  private_key=$(echo "$seed_response" | jq -r '.nsec')
  
  if [[ "$private_key" == "null" || -z "$private_key" ]]; then
    log "ERROR: Failed to generate private key"
    exit 1
  fi
  
  log "Generated private key"
  
  # Create wallet
  log "Creating Fulmine wallet..."
  curl -s -X POST http://localhost:7003/api/v1/wallet/create \
       -H "Content-Type: application/json" \
       -d "{\"private_key\": \"$private_key\", \"password\": \"password\", \"server_url\": \"http://ark:7070\"}" >/dev/null
  
  # Unlock wallet
  log "Unlocking Fulmine wallet..."
  curl -s -X POST http://localhost:7003/api/v1/wallet/unlock \
       -H "Content-Type: application/json" \
       -d '{"password": "password"}' >/dev/null
  
  sleep 2

  # Get wallet address with retry
  log "Getting Fulmine wallet address..."
  local fulmine_address=""
  for i in {1..5}; do
    local address_response=$(curl -s -X GET http://localhost:7003/api/v1/address)
    fulmine_address=$(echo "$address_response" | jq -r '.address' | sed 's/bitcoin://' | sed 's/?ark=.*//')
    
    if [[ "$fulmine_address" != "null" && -n "$fulmine_address" ]]; then
      log "Fulmine address: $fulmine_address"
      break
    fi
    
    log "Address not ready yet (attempt $i/5), waiting..."
    sleep 2
  done

  if [[ "$fulmine_address" == "null" || -z "$fulmine_address" ]]; then
    log "ERROR: Failed to get valid Fulmine wallet address"
    exit 1
  fi

  # Fund Fulmine wallet
  faucet "$fulmine_address" 0.01
  sleep 5

  # Settle
  log "Settling Fulmine wallet..."
  curl -s -X GET http://localhost:7003/api/v1/settle >/dev/null
  
  log "✓ Fulmine wallet setup completed!"
}

setup_ark_wallet() {
  log "Setting up Ark wallet..."
  
  wait_for_service "http://localhost:7070/health" "Ark daemon" 60 || exit 1

  # Get ark address
  log "Getting Ark address..."
  ark_address=$(docker exec -t ark ark receive --password secret | jq -r '.onchain_address')
  log "Ark address: $ark_address"

  # Fund ark wallet
  faucet "$ark_address" 2
  sleep 5

  # Create and redeem note
  log "Creating and redeeming Ark note..."
  note=$(docker exec -t ark arkd note --amount 100000000)
  docker exec -t ark ark redeem-notes -n "$note" --password secret
  
  log "✓ Ark wallet setup completed!"
}

# Main execution
cd "$SCRIPT_DIR"

log "🚀 Starting NArk development environment setup..."
log ""
log "ℹ️  This script assumes the Aspire AppHost is already running"
log "ℹ️  Start it with: dotnet run --project NArk.AppHost"
log ""

# Wait for core services
wait_for_service "http://localhost:3000" "Chopsticks" 60 || exit 1
wait_for_service "http://localhost:7070/health" "Ark daemon" 60 || exit 1

# Setup wallets
setup_ark_wallet
setup_fulmine_wallet
setup_lnd_wallet

log ""
log "✅ Development environment ready!"
log ""
log "Services available at:"
log "  • Ark daemon:              http://localhost:7070"
log "  • Ark wallet:              http://localhost:6060"
log "  • Boltz API:               http://localhost:9001"
log "  • Boltz gRPC:              http://localhost:9000"
log "  • Boltz WebSocket:         ws://localhost:9004"
log "  • Boltz CORS proxy:        http://localhost:9069"
log "  • Fulmine gRPC:            http://localhost:7002"
log "  • Fulmine API:             http://localhost:7003"
log "  • Boltz LND gRPC:          localhost:10010"
log "  • User LND gRPC:           localhost:10009"
log "  • Chopsticks (Explorer):   http://localhost:3000"
log "  • NBXplorer:               http://localhost:32838"
log "  • Esplora:                 http://localhost:5000"
log "