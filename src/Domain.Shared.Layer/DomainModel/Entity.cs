namespace Domain.Shared.Layer.DomainModel;

/// <summary>
/// Base type of every entity: a Guid identity (golden rule 13) and the six audit columns
/// (golden rule 19). The audit setters are private; only the audit interceptor writes them
/// (ADR-0016), through the names in <see cref="Constants.AuditColumns"/>.
/// </summary>
public abstract class Entity : IFullAudited
{
    /// <summary>Constructor for the persistence layer (materialization).</summary>
    protected Entity()
    {
    }

    /// <summary>Creates an entity with the given identity (generated with Guid.CreateVersion7, ADR-0010).</summary>
    protected Entity(Guid id)
    {
        Id = id;
    }

    /// <summary>The identity of the entity.</summary>
    public Guid Id { get; protected set; }

    /// <inheritdoc />
    public Guid? CreatorId { get; private set; }

    /// <inheritdoc />
    public DateTime? CreationTime { get; private set; }

    /// <inheritdoc />
    public Guid? LastModifierId { get; private set; }

    /// <inheritdoc />
    public DateTime? LastModificationTime { get; private set; }

    /// <inheritdoc />
    public Guid? DeletedBy { get; private set; }

    /// <inheritdoc />
    public DateTime? DeletedAt { get; private set; }
}
