using NBitcoin;
using NBitcoin.Scripting;
using NBitcoin.Secp256k1;

namespace NArk.Abstractions.Wallets;

public interface ISigningEntity
{
    Task<Dictionary<string, string>> GetMetadata();
    Task<string> GetFingerprint();
    Task<OutputDescriptor> GetOutputDescriptor();
    Task<ECPubKey> GetPublicKey();
    Task<SignResult> SignData(uint256 hash);
}