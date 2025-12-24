using NArk.Abstractions;
using NBitcoin;

namespace NArk.Services;

public interface ISpendingService
{
    Task<uint256> Spend(string walletId, ArkTxOut[] outputs, CancellationToken cancellationToken = default);
}