namespace NArk.Abstractions.Batches;

public record TreeNoncesAggregatedEvent(string Id, Dictionary<string, string> TreeNonces) : BatchEvent;