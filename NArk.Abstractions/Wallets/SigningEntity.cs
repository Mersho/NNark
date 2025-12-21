using NBitcoin;
using NBitcoin.Secp256k1;

namespace NArk.Abstractions.Wallets;

public interface ISigningEntity
{
    Task<Dictionary<string, string>> GetMetadata();
    Task<string> GetFingerprint();
    Task<ECPubKey> GetPublicKey();
    Task<SignResult> SignData(uint256 hash);
}