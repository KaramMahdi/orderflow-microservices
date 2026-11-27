namespace Domain.Shared.Layer.Repositories;

/// <summary>
/// Saves every change tracked in the current unit of work in one transaction.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Saves the pending changes.</summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
