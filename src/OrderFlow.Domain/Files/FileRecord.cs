using System.Globalization;
using Domain.Shared.Layer.DomainModel;
using Domain.Shared.Layer.Errors;
using OrderFlow.Domain.Files.Events;
using OrderFlow.Domain.Shared.Files;

namespace OrderFlow.Domain.Files;

/// <summary>
/// The database record of a file: metadata only, never content (golden rule 14, ADR-0011, ADR-0012).
/// Invariants: the storage name is the record id plus a validated extension and never changes; the
/// storage path is {Module}/{yyyy}/{MM}; Pending becomes Active (with size and SHA-256), Rejected or
/// Failed, and nothing leaves those three.
/// </summary>
public sealed class FileRecord : AggregateRoot
{
    private FileRecord(Guid id, string storageName, string storagePath, string originalName, string contentType, string moduleName, string entityName, Guid entityId)
        : base(id)
    {
        StorageName = storageName;
        StoragePath = storagePath;
        OriginalName = originalName;
        ContentType = contentType;
        ModuleName = moduleName;
        EntityName = entityName;
        EntityId = entityId;
        Status = FileStatus.Pending;
    }

    /// <summary>The unique name on disk: the record id plus the extension.</summary>
    public string StorageName { get; private set; }

    /// <summary>The relative folder: {Module}/{yyyy}/{MM}. The root comes from Options.</summary>
    public string StoragePath { get; private set; }

    /// <summary>The name the user uploaded; for display and downloads only, never in a path.</summary>
    public string OriginalName { get; private set; }

    /// <summary>The validated content type.</summary>
    public string ContentType { get; private set; }

    /// <summary>The size in bytes; zero until the content is saved.</summary>
    public long SizeBytes { get; private set; }

    /// <summary>The SHA-256 of the content (64 lowercase hexadecimal characters); null until the content is saved.</summary>
    public string? Sha256 { get; private set; }

    /// <summary>The state. Only Active is served.</summary>
    public FileStatus Status { get; private set; }

    /// <summary>The module of the owning record (owner link).</summary>
    public string ModuleName { get; private set; }

    /// <summary>The entity of the owning record (owner link).</summary>
    public string EntityName { get; private set; }

    /// <summary>The id of the owning record (owner link). No foreign key.</summary>
    public Guid EntityId { get; private set; }

    /// <summary>Creates a Pending record before the content is written (ADR-0012 step 1).</summary>
    /// <param name="originalName">The uploaded file name.</param>
    /// <param name="contentType">The validated content type.</param>
    /// <param name="extension">The extension taken from the validated content type, with its dot (for example ".pdf").</param>
    /// <param name="moduleName">The module of the owning record.</param>
    /// <param name="entityName">The entity of the owning record.</param>
    /// <param name="entityId">The id of the owning record.</param>
    /// <param name="timeProvider">The clock, for the date folders.</param>
    public static FileRecord CreatePending(
        string? originalName,
        string? contentType,
        string? extension,
        string? moduleName,
        string? entityName,
        Guid entityId,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        var validOriginalName = Check.Text(originalName, nameof(OriginalName), FileConstants.OriginalNameMaxLength, FilesMessageKeys.FieldRequired, FilesMessageKeys.FieldTooLong);
        var validContentType = Check.Text(contentType, nameof(ContentType), FileConstants.ContentTypeMaxLength, FilesMessageKeys.FieldRequired, FilesMessageKeys.FieldTooLong);
        var validModuleName = Check.Text(moduleName, nameof(ModuleName), FileConstants.ModuleNameMaxLength, FilesMessageKeys.FieldRequired, FilesMessageKeys.FieldTooLong);
        var validEntityName = Check.Text(entityName, nameof(EntityName), FileConstants.EntityNameMaxLength, FilesMessageKeys.FieldRequired, FilesMessageKeys.FieldTooLong);
        Check.NotEmpty(entityId, nameof(EntityId), FilesMessageKeys.FieldRequired);
        var validExtension = ValidExtension(extension);

        var id = Guid.CreateVersion7();
        var now = timeProvider.GetLocalNow().DateTime;
        var storageName = string.Concat(id.ToString(), validExtension);
        var storagePath = string.Join(
            FileConstants.PathSeparator,
            validModuleName,
            now.ToString(FileConstants.YearFolderFormat, CultureInfo.InvariantCulture),
            now.ToString(FileConstants.MonthFolderFormat, CultureInfo.InvariantCulture));

        return new FileRecord(id, storageName, storagePath, validOriginalName, validContentType, validModuleName, validEntityName, entityId);
    }

    /// <summary>The content is saved and clean (ADR-0012 step 4). Raises <see cref="FileStored"/>.</summary>
    /// <param name="sizeBytes">The size measured while streaming.</param>
    /// <param name="sha256">The SHA-256 measured while streaming.</param>
    public void MarkActive(long sizeBytes, string? sha256)
    {
        EnsurePending();

        if (sizeBytes <= 0)
        {
            throw new ErrorException(ErrorCode.ValidationFailed, FilesMessageKeys.SizeNotPositive, sizeBytes);
        }

        if (sha256 is null || sha256.Length != FileConstants.Sha256Length || !sha256.All(char.IsAsciiHexDigitLower))
        {
            throw new ErrorException(ErrorCode.ValidationFailed, FilesMessageKeys.Sha256Invalid);
        }

        SizeBytes = sizeBytes;
        Sha256 = sha256;
        Status = FileStatus.Active;
        AddDomainEvent(new FileStored(Id, ModuleName, EntityName, EntityId));
    }

    /// <summary>The content is infected or not allowed. Never served; stays on disk (ADR-0005).</summary>
    public void Reject()
    {
        EnsurePending();
        Status = FileStatus.Rejected;
    }

    /// <summary>The record stayed Pending too long (the cleanup job). Nothing is deleted.</summary>
    public void MarkFailed()
    {
        EnsurePending();
        Status = FileStatus.Failed;
    }

    private static string ValidExtension(string? extension)
    {
        var value = extension?.Trim().ToLowerInvariant();

        if (value is null
            || value.Length < 2
            || value.Length > FileConstants.ExtensionMaxLength
            || value[0] != FileConstants.ExtensionPrefix
            || !value[1..].All(char.IsAsciiLetterOrDigit))
        {
            throw new ErrorException(ErrorCode.ValidationFailed, FilesMessageKeys.ExtensionInvalid, extension);
        }

        return value;
    }

    private void EnsurePending()
    {
        if (Status != FileStatus.Pending)
        {
            throw new ErrorException(ErrorCode.Conflict, FilesMessageKeys.FileNotPending, Id, Status);
        }
    }
}
