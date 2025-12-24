using NBitcoin;

namespace NArk.Abstractions.Intents;

public interface IIntentStorage
{
    public event EventHandler? IntentChanged;

    public Task SaveIntent(string walletId, ArkIntent intent, CancellationToken cancellationToken = default);
    public Task<IReadOnlyCollection<ArkIntent>> GetIntents(string walletId);
    public Task<ArkIntent?> GetIntentByInternalId(Guid internalId);
    public Task<ArkIntent?> GetIntentByIntentId(string walletId, string intentId);
    public Task<IReadOnlyCollection<ArkIntent>> GetIntentsByInputs(string walletId, OutPoint[] inputs, bool pendingOnly = true);
    public Task<IReadOnlyCollection<ArkIntent>> GetUnsubmittedIntents();
    public Task<IReadOnlyCollection<ArkIntent>> GetActiveIntents();
}