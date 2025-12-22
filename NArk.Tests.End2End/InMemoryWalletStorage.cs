using System.Collections.Concurrent;
using NArk.Abstractions.Contracts;
using NArk.Abstractions.Wallets;

namespace NArk.Tests.End2End;

public class InMemoryWalletStorage: IWalletStorage
{
    private readonly ConcurrentDictionary<string, ArkWallet> _wallets = new();
    
    public Task<IReadOnlySet<ArkWallet>> LoadAllWallets()
    {
        return Task.FromResult<IReadOnlySet<ArkWallet>>(_wallets.Values.ToHashSet());
    }

    public Task<ArkWallet> LoadWallet(string walletIdentifierOrFingerprint)
    {
        if (_wallets.TryGetValue(walletIdentifierOrFingerprint, out var wallet))
            return Task.FromResult(wallet);

        return 
            Task.FromResult(_wallets
                .Values
                .First(w => w.WalletFingerprint == walletIdentifierOrFingerprint));
    }

    public Task SaveWallet(string walletId, ArkWallet arkWallet, string? walletFingerprint = null)
    {
        _wallets[walletId] = arkWallet;
        return Task.CompletedTask;
    }
}