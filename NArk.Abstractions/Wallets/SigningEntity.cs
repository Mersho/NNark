using NBitcoin;
using NBitcoin.Scripting;
using NBitcoin.Secp256k1;
using NBitcoin.Secp256k1.Musig;

namespace NArk.Abstractions.Wallets;

public interface ISigningEntity
{
    Task<Dictionary<string, string>> GetMetadata();
    Task<string> GetFingerprint();
    Task<OutputDescriptor> GetOutputDescriptor();
    Task<ECPubKey> GetPublicKey();
    Task<ECPrivKey> DerivePrivateKey();
    Task<SignResult> SignData(uint256 hash);
    Task<MusigPartialSignature> SignMusig(MusigContext context,
        MusigPrivNonce nonce,
        CancellationToken cancellationToken = default);
}