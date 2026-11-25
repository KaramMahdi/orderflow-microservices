namespace Domain.Shared.Layer.Dtos;

/// <summary>
/// One page of a list query and the total number of rows that match it (ADR-0006).
/// </summary>
/// <typeparam name="T">The item DTO type.</typeparam>
/// <param name="Items">The rows of the page.</param>
/// <param name="TotalCount">The number of rows that match the query, across all pages.</param>
public sealed record PagedResultDto<T>(IReadOnlyList<T> Items, int TotalCount);
