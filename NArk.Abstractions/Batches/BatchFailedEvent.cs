namespace NArk.Abstractions.Batches;

public record BatchFailedEvent(string Id, string Reason) : BatchEvent;