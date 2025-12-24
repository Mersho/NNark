namespace NArk.Abstractions.Batches;

public record BatchFinalizedEvent(string CommitmentTxId, string Id) : BatchEvent;