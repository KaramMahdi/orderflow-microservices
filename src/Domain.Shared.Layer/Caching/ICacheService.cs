namespace Domain.Shared.Layer.Caching;

/// <summary>
/// The cache Application code sees (Phase 2). Implemented once in Shared.Infrastructure (ADR-0017), so
/// no inner layer references a Redis package. Every entry has a time to live; keys come from a CacheKeys
/// class and times to live from Options.
/// </summary>
/// <remarks>
/// TODO(Phase 2): the behavior when the cache is unavailable (R-007), stale entries (R-008) and
/// stampede protection (R-009) are Open risks; their ADRs may extend this contract.
/// </remarks>
public interface ICacheService
{
    /// <summary>Reads an entry.</summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">Cancels the read.</param>
    /// <returns>The cached value, or null when the key is not cached.</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>Writes an entry that expires after the given time to live.</summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="timeToLive">How long the entry lives; every entry has one.</param>
    /// <param name="cancellationToken">Cancels the write.</param>
    Task SetAsync<T>(string key, T value, TimeSpan timeToLive, CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>Removes an entry, for example when the data behind it changes.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">Cancels the removal.</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
