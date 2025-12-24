namespace NArk.Abstractions.Batches;

public record TreeSignatureEvent(int BatchIndex, string Id, string Signature, IReadOnlyCollection<string> Topic, string TxId): Event;