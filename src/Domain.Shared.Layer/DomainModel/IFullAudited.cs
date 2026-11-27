namespace Domain.Shared.Layer.DomainModel;

/// <summary>
/// The six audit columns every table carries (golden rule 19, ADR-0016).
/// All values are nullable and are written only by the audit interceptor.
/// </summary>
public interface IFullAudited
{
    /// <summary>The user who created the row; null when no user was present (job, seed).</summary>
    Guid? CreatorId { get; }

    /// <summary>When the row was created, in server local time (ADR-0004).</summary>
    DateTime? CreationTime { get; }

    /// <summary>The user who last modified the row; null when no user was present.</summary>
    Guid? LastModifierId { get; }

    /// <summary>When the row was last modified, in server local time.</summary>
    DateTime? LastModificationTime { get; }

    /// <summary>The user who soft deleted the row; null when no user was present.</summary>
    Guid? DeletedBy { get; }

    /// <summary>When the row was soft deleted; null while the row is not deleted (ADR-0005).</summary>
    DateTime? DeletedAt { get; }
}
