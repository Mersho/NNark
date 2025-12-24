namespace NArk.Abstractions.Batches;

public record TreeNoncesEvent(string Id, Dictionary<string, string> Nonces, IReadOnlyCollection<string> Topic, string TxId): Event;