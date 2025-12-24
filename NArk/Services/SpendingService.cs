using NArk.Abstractions;
using NArk.Abstractions.Contracts;
using NArk.Abstractions.VTXOs;
using NArk.Helpers;
using NArk.Transactions;
using NArk.Transport;
using NBitcoin;

namespace NArk.Services;

public class SpendingService(
    IVtxoStorage vtxoStorage,
    IContractStorage contractStorage,
    ISigningService signingService,
    IContractService paymentService,
    IClientTransport transport
)
{
    public async Task<uint256> Spend(string walletId, ArkTxOut[] outputs, CancellationToken cancellationToken = default)
    {
        var serverInfo = await transport.GetServerInfoAsync(cancellationToken);

        var outputsSumInSatoshis = outputs.Sum(o => o.Value);

        // Check if any output is explicitly subdust (the user wants to send subdust amount)
        var hasExplicitSubdustOutput = outputs.Count(o => o.Value < serverInfo.Dust);

        var contracts = await contractStorage.LoadActiveContracts([walletId]);
        var contractByScript =
            contracts
                .GroupBy(c => c.Script)
                .ToDictionary(g => g.Key, g => g.First());
        var vtxos = await vtxoStorage.GetVtxosByScripts([.. contracts.Select(c => c.Script)]);
        var vtxosByContracts =
            vtxos
                .GroupBy(v => contractByScript[v.Script]);

        HashSet<ArkPsbtSigner> coins = [];
        foreach (var vtxosByContract in vtxosByContracts)
        {
            foreach (var vtxo in vtxosByContract)
            {
                coins.Add(await signingService.GetVtxoPsbtSignerByContract(vtxosByContract.Key, vtxo));
            }
        }

        var selectedCoins = CoinSelectionHelper.SelectCoins([.. coins], outputsSumInSatoshis, serverInfo.Dust, hasExplicitSubdustOutput);

        var totalInput = selectedCoins.Sum(x => x.Coin.TxOut.Value);
        var change = totalInput - outputsSumInSatoshis;

        // Only derive a new change address if we actually need change
        // This is important for HD wallets as it consumes a derivation index
        ArkAddress? changeAddress = null;
        var needsChange = change >= serverInfo.Dust ||
                          (change > 0L && (hasExplicitSubdustOutput + 1) <= TransactionHelpers.MaxOpReturnOutputs);

        if (needsChange)
        {
            // GetDestination uses DerivePaymentContract, which saves the contract to DB
            changeAddress = (await paymentService.DerivePaymentContract(walletId)).GetArkAddress();
        }

        // Add change output if it's at or above the dust threshold
        if (change >= serverInfo.Dust)
        {
            outputs = outputs.Concat([new ArkTxOut(ArkTxOutType.Vtxo, Money.Satoshis(change), changeAddress!)]).ToArray();
        }
        else if (change > 0 && (hasExplicitSubdustOutput + 1) <= TransactionHelpers.MaxOpReturnOutputs)
        {
            outputs = outputs.Concat([new ArkTxOut(ArkTxOutType.Vtxo, Money.Satoshis(change), changeAddress!)]).ToArray();
        }

        var transactionBuilder = new TransactionHelpers.ArkTransactionBuilder(transport);

        return await transactionBuilder.ConstructAndSubmitArkTransaction(selectedCoins, outputs, cancellationToken);
    }
}