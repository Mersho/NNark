using NArk.Abstractions.Contracts;
using NArk.Abstractions.VTXOs;
using NArk.Transactions;
using NBitcoin;

namespace NArk.Services;

public interface ISigningService
{
    Task<ArkPsbtSigner> GetPsbtSigner(ArkVtxo vtxo);
    Task<ArkPsbtSigner> GetPsbtSigner(ArkCoin coin);
    Task<ArkPsbtSigner> GetVtxoPsbtSignerByContract(ArkContractEntity contractEntity, ArkVtxo vtxo);
}