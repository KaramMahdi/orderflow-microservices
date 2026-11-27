using Domain.Shared.Layer.Constants;
using Domain.Shared.Layer.Errors;
using OrderFlow.Domain.Files;
using OrderFlow.Domain.Files.Specifications;
using OrderFlow.Domain.Shared.Files;
using OrderFlow.Domain.Tests.TestSupport;

namespace OrderFlow.Domain.Tests.Files;

/// <summary>The remaining FileRecord invariants.</summary>
public sealed class FileRecordInvariantTests
{
    private const string OriginalName = "invoice.pdf";
    private const string ContentType = "application/pdf";
    private const string Extension = ".pdf";
    private const string EntityName = "Order";

    private readonly FixedTimeProvider _clock = new(new DateTimeOffset(2026, 11, 27, 10, 0, 0, TimeSpan.Zero));

    [Fact]
    public void TheMetadataIsRequired()
    {
        var entityId = Guid.CreateVersion7();

        ErrorAssert.Throws(ErrorCode.ValidationFailed, FilesMessageKeys.FieldRequired, () => Create(null, ContentType, Modules.Orders, EntityName, entityId));
        ErrorAssert.Throws(ErrorCode.ValidationFailed, FilesMessageKeys.FieldRequired, () => Create(OriginalName, " ", Modules.Orders, EntityName, entityId));
        ErrorAssert.Throws(ErrorCode.ValidationFailed, FilesMessageKeys.FieldRequired, () => Create(OriginalName, ContentType, "", EntityName, entityId));
        ErrorAssert.Throws(ErrorCode.ValidationFailed, FilesMessageKeys.FieldRequired, () => Create(OriginalName, ContentType, Modules.Orders, null, entityId));
        ErrorAssert.Throws(ErrorCode.ValidationFailed, FilesMessageKeys.FieldRequired, () => Create(OriginalName, ContentType, Modules.Orders, EntityName, Guid.Empty));
    }

    [Fact]
    public void TheMetadataHasMaximumLengths()
    {
        var entityId = Guid.CreateVersion7();

        ErrorAssert.Throws(ErrorCode.ValidationFailed, FilesMessageKeys.FieldTooLong, () => Create(new string('n', FileConstants.OriginalNameMaxLength + 1), ContentType, Modules.Orders, EntityName, entityId));
        ErrorAssert.Throws(ErrorCode.ValidationFailed, FilesMessageKeys.FieldTooLong, () => Create(OriginalName, new string('c', FileConstants.ContentTypeMaxLength + 1), Modules.Orders, EntityName, entityId));
        ErrorAssert.Throws(ErrorCode.ValidationFailed, FilesMessageKeys.FieldTooLong, () => Create(OriginalName, ContentType, new string('m', FileConstants.ModuleNameMaxLength + 1), EntityName, entityId));
        ErrorAssert.Throws(ErrorCode.ValidationFailed, FilesMessageKeys.FieldTooLong, () => Create(OriginalName, ContentType, Modules.Orders, new string('e', FileConstants.EntityNameMaxLength + 1), entityId));
    }

    [Fact]
    public void TheOriginalNameNeverReachesThePath()
    {
        var file = Create("../../etc/passwd.pdf", ContentType, Modules.Orders, EntityName, Guid.CreateVersion7());

        Assert.DoesNotContain("..", file.StorageName, StringComparison.Ordinal);
        Assert.DoesNotContain("passwd", file.StoragePath, StringComparison.Ordinal);
        Assert.Equal("../../etc/passwd.pdf", file.OriginalName);
    }

    [Fact]
    public void TheStorageNameAndPathFitTheirColumns()
    {
        var file = Create(OriginalName, ContentType, new string('m', FileConstants.ModuleNameMaxLength), EntityName, Guid.CreateVersion7());

        Assert.True(file.StorageName.Length <= FileConstants.StorageNameMaxLength);
        Assert.True(file.StoragePath.Length <= FileConstants.StoragePathMaxLength);
    }

    [Fact]
    public void ActiveNeedsAHash()
    {
        var file = Create(OriginalName, ContentType, Modules.Orders, EntityName, Guid.CreateVersion7());

        ErrorAssert.Throws(ErrorCode.ValidationFailed, FilesMessageKeys.Sha256Invalid, () => file.MarkActive(1, null));
        ErrorAssert.Throws(ErrorCode.ValidationFailed, FilesMessageKeys.Sha256Invalid, () => file.MarkActive(1, "abc"));
        Assert.Equal(FileStatus.Pending, file.Status);
    }

    [Fact]
    public void SpecificationsSelectOwnerActiveAndStale()
    {
        var entityId = Guid.CreateVersion7();
        var file = Create(OriginalName, ContentType, Modules.Orders, EntityName, entityId);

        Assert.True(new FilesOfOwnerSpecification(Modules.Orders, EntityName, entityId).IsSatisfiedBy(file));
        Assert.False(new FilesOfOwnerSpecification(Modules.Catalog, EntityName, entityId).IsSatisfiedBy(file));
        Assert.False(new ActiveFilesSpecification().IsSatisfiedBy(file));
    }

    private FileRecord Create(string? originalName, string? contentType, string? moduleName, string? entityName, Guid entityId) =>
        FileRecord.CreatePending(originalName, contentType, Extension, moduleName, entityName, entityId, _clock);
}
