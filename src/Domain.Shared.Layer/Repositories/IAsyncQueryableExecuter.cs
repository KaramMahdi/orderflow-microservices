namespace Domain.Shared.Layer.Repositories;

/// <summary>
/// Executes a composed <see cref="IQueryable{T}"/> asynchronously, so the Application layer never
/// references EF Core (ADR-0007). Implemented in EntityFrameworkCore.
/// </summary>
public interface IAsyncQueryableExecuter
{
    /// <summary>Executes the query and returns every row. List queries must be paged first (ADR-0006).</summary>
    Task<List<T>> ToListAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default);

    /// <summary>Executes the query and returns the number of rows.</summary>
    Task<int> CountAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default);

    /// <summary>Executes the query and returns the first row, or null when there is none.</summary>
    Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default);
}
