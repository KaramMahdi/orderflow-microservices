using Domain.Shared.Layer.DomainModel;

namespace OrderFlow.Domain.Files.Events;

/// <summary>A file was saved, scanned clean and is Active.</summary>
/// <param name="FileRecordId">The file record.</param>
/// <param name="ModuleName">The module of the owning record.</param>
/// <param name="EntityName">The entity of the owning record.</param>
/// <param name="EntityId">The id of the owning record.</param>
public sealed record FileStored(Guid FileRecordId, string ModuleName, string EntityName, Guid EntityId) : IDomainEvent;
