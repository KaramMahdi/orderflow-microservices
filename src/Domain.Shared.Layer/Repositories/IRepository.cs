using Domain.Shared.Layer.DomainModel;

namespace Domain.Shared.Layer.Repositories;

/// <summary>
/// The repository of one aggregate root (golden rule 3). Reads compose the returned
/// <see cref="IQueryable{T}"/> and execute it through <see cref="IAsyncQueryableExecuter"/> (ADR-0007).
/// </summary>
/// <typeparam name="TEntity">The aggregate root type.</typeparam>
public interface IRepository<TEntity>
    where TEntity : AggregateRoot
{
    /// <summary>Returns a query over the aggregate roots; soft-deleted rows are hidden (ADR-0005).</summary>
    IQueryable<TEntity> GetQueryable();
}
