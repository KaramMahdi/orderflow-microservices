using System.Linq.Expressions;
using Domain.Shared.Layer.Specifications;
using OrderFlow.Domain.Shared.Files;

namespace OrderFlow.Domain.Files.Specifications;

/// <summary>The files of a business record (owner link).</summary>
/// <param name="moduleName">The module.</param>
/// <param name="entityName">The entity.</param>
/// <param name="entityId">The record id.</param>
public sealed class FilesOfOwnerSpecification(string moduleName, string entityName, Guid entityId) : Specification<FileRecord>
{
    /// <inheritdoc />
    public override Expression<Func<FileRecord, bool>> ToExpression() =>
        file => file.ModuleName == moduleName && file.EntityName == entityName && file.EntityId == entityId;
}

/// <summary>The files that can be served (Active only).</summary>
public sealed class ActiveFilesSpecification : Specification<FileRecord>
{
    /// <inheritdoc />
    public override Expression<Func<FileRecord, bool>> ToExpression() => file => file.Status == FileStatus.Active;
}

/// <summary>The Pending records created before a cut-off (the cleanup job, ADR-0012).</summary>
/// <param name="createdBefore">The cut-off, server local time.</param>
public sealed class StalePendingFilesSpecification(DateTime createdBefore) : Specification<FileRecord>
{
    /// <inheritdoc />
    public override Expression<Func<FileRecord, bool>> ToExpression() =>
        file => file.Status == FileStatus.Pending && file.CreationTime < createdBefore;
}
