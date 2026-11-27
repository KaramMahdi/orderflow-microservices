namespace Domain.Shared.Layer.Storage;

/// <summary>
/// What <see cref="IFileStorage.SaveAsync"/> measured while streaming the content (ADR-0011, ADR-0012).
/// </summary>
/// <param name="Size">The number of bytes written.</param>
/// <param name="Sha256">The SHA-256 of the content, as 64 lowercase hexadecimal characters.</param>
public sealed record FileWriteResult(long Size, string Sha256);
