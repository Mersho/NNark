using NBitcoin;

namespace NArk.Abstractions.Batches;

public record BatchStartedEvent(string Id, Sequence BatchExpiry, IReadOnlyCollection<string> IntentIdHashes): Event;