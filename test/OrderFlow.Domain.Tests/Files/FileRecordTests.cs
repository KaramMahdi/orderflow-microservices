using Domain.Shared.Layer.Constants;
using Domain.Shared.Layer.Errors;
using OrderFlow.Domain.Files;
using OrderFlow.Domain.Files.Events;
using OrderFlow.Domain.Shared.Files;
using OrderFlow.Domain.Tests.TestSupport;

namespace OrderFlow.Domain.Tests.Files;

/// <summary>FileRecord naming and status (ADR-0011, ADR-0012).</summary>
public sealed class FileRecordTests
{
    private const string ValidSha256 = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

    private readonly FixedTimeProvider _clock = new(new DateTimeOffset(2026, 3, 5, 10, 0, 0, TimeSpan.Zero));

    [Fact]
    public void ThePendingRecordIsNamedByItsIdAndFiledByModuleAndDate()
    {
        var file = CreatePending(" .PDF ");

        Assert.Equal(FileStatus.Pending, file.Status);
        Assert.Equal($"{file.Id}.pdf", file.StorageName);
        Assert.Equal("Orders/2026/03", file.StoragePath);
        Assert.Equal(0, file.SizeBytes);
        Assert.Null(file.Sha256);
    }

    [Theory]
    [InlineData("pdf")]
    [InlineData(".")]
    [InlineData("./pdf")]
    [InlineData(".verylongext")]
    public void AnInvalidExtensionIsRejected(string extension)
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, FilesMessageKeys.ExtensionInvalid, () => CreatePending(extension));
    }

    [Fact]
    public void ActiveKeepsTheSizeAndHashAndRaisesFileStored()
    {
        var file = CreatePending(".pdf");

        file.MarkActive(1024, ValidSha256);

        Assert.Equal(FileStatus.Active, file.Status);
        Assert.Equal(1024, file.SizeBytes);
        Assert.Equal(ValidSha256, file.Sha256);
        Assert.IsType<FileStored>(Assert.Single(file.DomainEvents));
    }

    [Fact]
    public void ActiveNeedsAValidHashAndSize()
    {
        var file = CreatePending(".pdf");

        ErrorAssert.Throws(ErrorCode.ValidationFailed, FilesMessageKeys.Sha256Invalid, () => file.MarkActive(1, ValidSha256.ToUpperInvariant()));
        ErrorAssert.Throws(ErrorCode.ValidationFailed, FilesMessageKeys.SizeNotPositive, () => file.MarkActive(0, ValidSha256));
    }

    [Fact]
    public void OnlyAPendingRecordChangesStatus()
    {
        var rejected = CreatePending(".pdf");
        rejected.Reject();
        var failed = CreatePending(".pdf");
        failed.MarkFailed();

        ErrorAssert.Throws(ErrorCode.Conflict, FilesMessageKeys.FileNotPending, () => rejected.MarkActive(1, ValidSha256));
        ErrorAssert.Throws(ErrorCode.Conflict, FilesMessageKeys.FileNotPending, failed.Reject);
        Assert.Equal(FileStatus.Rejected, rejected.Status);
        Assert.Equal(FileStatus.Failed, failed.Status);
    }

    private FileRecord CreatePending(string extension) =>
        FileRecord.CreatePending("invoice.pdf", "application/pdf", extension, Modules.Orders, "Order", Guid.CreateVersion7(), _clock);
}
