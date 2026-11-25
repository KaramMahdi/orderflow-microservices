using Domain.Shared.Layer.DomainModel;

namespace Domain.Shared.Layer.Constants;

/// <summary>
/// Names of the six audit columns (golden rule 19), built from <see cref="IFullAudited"/> with nameof,
/// so a rename breaks the build instead of the audit interceptor.
/// </summary>
public static class AuditColumns
{
    /// <summary>Column of the user who created the row.</summary>
    public const string CreatorId = nameof(IFullAudited.CreatorId);

    /// <summary>Column of the creation time.</summary>
    public const string CreationTime = nameof(IFullAudited.CreationTime);

    /// <summary>Column of the user who last modified the row.</summary>
    public const string LastModifierId = nameof(IFullAudited.LastModifierId);

    /// <summary>Column of the last modification time.</summary>
    public const string LastModificationTime = nameof(IFullAudited.LastModificationTime);

    /// <summary>Column of the user who soft deleted the row.</summary>
    public const string DeletedBy = nameof(IFullAudited.DeletedBy);

    /// <summary>Column of the soft delete time.</summary>
    public const string DeletedAt = nameof(IFullAudited.DeletedAt);
}
