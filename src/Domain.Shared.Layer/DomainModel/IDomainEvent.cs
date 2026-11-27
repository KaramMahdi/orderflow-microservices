namespace Domain.Shared.Layer.DomainModel;

/// <summary>
/// Something that happened inside an aggregate. Raised by the aggregate root and dispatched after a
/// successful save (glossary: Domain event). Events carry ids and values only, never entities.
/// </summary>
public interface IDomainEvent;
