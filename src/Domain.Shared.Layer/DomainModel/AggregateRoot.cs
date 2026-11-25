namespace Domain.Shared.Layer.DomainModel;

/// <summary>
/// The entity that guards the invariants of its aggregate. The only entry point for changes
/// and the only type that has a repository (one repository per aggregate root).
/// </summary>
public abstract class AggregateRoot : Entity
{
    /// <summary>Constructor for the persistence layer (materialization).</summary>
    protected AggregateRoot()
    {
    }

    /// <summary>Creates an aggregate root with the given identity.</summary>
    protected AggregateRoot(Guid id)
        : base(id)
    {
    }
}
