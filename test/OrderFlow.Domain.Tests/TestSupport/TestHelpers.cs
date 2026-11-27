using Domain.Shared.Layer.Errors;
using Domain.Shared.Layer.Localization;
using Domain.Shared.Layer.Repositories;

namespace OrderFlow.Domain.Tests.TestSupport;

/// <summary>A clock fixed at one instant, with UTC as the local zone so local time equals the instant.</summary>
/// <param name="now">The instant.</param>
internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = now;

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

    public override DateTimeOffset GetUtcNow() => Now;
}

/// <summary>Runs queries with LINQ to Objects, standing in for the EF Core executer.</summary>
internal sealed class InMemoryQueryableExecuter : IAsyncQueryableExecuter
{
    public Task<List<T>> ToListAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) =>
        Task.FromResult(query.ToList());

    public Task<int> CountAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) =>
        Task.FromResult(query.Count());

    public Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) =>
        Task.FromResult(query.FirstOrDefault());
}

/// <summary>Assertions for <see cref="ErrorException"/>.</summary>
internal static class ErrorAssert
{
    public static ErrorException Throws(ErrorCode code, MessageKey key, Action action)
    {
        var exception = Assert.Throws<ErrorException>(action);
        Assert.Equal(code, exception.Code);
        Assert.Equal(key, exception.Key);

        return exception;
    }

    public static async Task<ErrorException> ThrowsAsync(ErrorCode code, MessageKey key, Func<Task> action)
    {
        var exception = await Assert.ThrowsAsync<ErrorException>(action);
        Assert.Equal(code, exception.Code);
        Assert.Equal(key, exception.Key);

        return exception;
    }
}
