namespace NArk.Abstractions.Batches;

public record BatchFinalizationEvent(string CommitmentTx, string Id): Event;