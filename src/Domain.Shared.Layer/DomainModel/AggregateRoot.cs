namespace Domain.Shared.Layer.DomainModel;

/// <summary>
/// The entity that guards the invariants of its aggregate. The only entry point for changes
/// and the only type that has a repository (one repository per aggregate root).
/// It collects the domain events it raises until they are dispatched.
/// </summary>
public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>Constructor for the persistence layer (materialization).</summary>
    protected AggregateRoot()
    {
    }

    /// <summary>Creates an aggregate root with the given identity.</summary>
    protected AggregateRoot(Guid id)
        : base(id)
    {
    }

    /// <summary>The domain events raised since the last dispatch, in the order they were raised.</summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>Forgets the raised events. Called after they are dispatched.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>Raises a domain event from inside the aggregate.</summary>
    /// <param name="domainEvent">The event.</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        _domainEvents.Add(domainEvent);
    }
}
