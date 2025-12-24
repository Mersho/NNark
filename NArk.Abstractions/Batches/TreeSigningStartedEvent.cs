namespace NArk.Abstractions.Batches;

public record TreeSigningStartedEvent(string UnsignedCommitmentTx, string Id, string[] CosignersPubkeys) : BatchEvent;