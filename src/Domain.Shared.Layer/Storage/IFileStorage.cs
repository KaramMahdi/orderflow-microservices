namespace Domain.Shared.Layer.Storage;

/// <summary>
/// The only way to touch file content (ADR-0011). Implemented once in Shared.Infrastructure (ADR-0017);
/// only that implementation changes when the storage moves to another backend.
/// Content is always streamed, never loaded whole into memory, and never deleted (ADR-0005, ADR-0012).
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Writes the stream under the given relative path and computes its size and SHA-256 while streaming.
    /// The path is <c>{Module}/{yyyy}/{MM}/{fileId}{ext}</c>; the root folder comes from Options (ADR-0011).
    /// An existing file is never overwritten: every storage name is a new Guid.
    /// </summary>
    /// <param name="relativePath">The storage path and name, relative to the configured root.</param>
    /// <param name="content">The content to write, read once from its current position.</param>
    /// <param name="cancellationToken">Cancels the write.</param>
    /// <returns>The number of bytes written and their SHA-256.</returns>
    Task<FileWriteResult> SaveAsync(string relativePath, Stream content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens the file for streaming reads. The caller disposes the returned stream.
    /// Access checks happen before this call, never inside the storage (ADR-0011).
    /// </summary>
    /// <param name="relativePath">The storage path and name, relative to the configured root.</param>
    /// <param name="cancellationToken">Cancels the open.</param>
    /// <returns>A read-only stream over the file content.</returns>
    Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default);
}
