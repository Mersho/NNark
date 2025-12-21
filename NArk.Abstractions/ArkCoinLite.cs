using NBitcoin;

namespace NArk.Abstractions;

public class ArkCoinLite(
    string walletIdentifier,
    OutPoint outPoint,
    TxOut txOut)
    : Coin(outPoint, txOut)
{
    public string WalletIdentifier { get; } = walletIdentifier;
}