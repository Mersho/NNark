using NBitcoin;

namespace NArk.Abstractions.Intents;

public interface IIntentStorage
{
    public event EventHandler? IntentChanged;
    
    public Task SaveIntent(string walletId, ArkIntent intent);
    public Task<IReadOnlyCollection<ArkIntent>> GetIntents(string walletId);
    public Task<ArkIntent?> GetIntentByInternalId(string walletId, int internalId);
    public Task<ArkIntent?> GetIntentByIntentId(string walletId, string intentId);
    public Task<IReadOnlyCollection<ArkIntent>> GetIntentsByInputs(OutPoint[] inputs, bool pendingOnly = true);
    public Task<IReadOnlyCollection<ArkIntent>> GetUnsubmittedIntents();
}