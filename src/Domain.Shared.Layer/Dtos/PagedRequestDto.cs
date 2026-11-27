namespace Domain.Shared.Layer.Dtos;

/// <summary>
/// The input of every list query: offset paging by page number and page size (ADR-0006).
/// A list input DTO in Application.Contracts derives from it.
/// </summary>
public class PagedRequestDto
{
    /// <summary>The number of the first page.</summary>
    public const int FirstPageNumber = 1;

    /// <summary>The page to read, starting at <see cref="FirstPageNumber"/>.</summary>
    public int PageNumber { get; set; } = FirstPageNumber;

    /// <summary>The number of rows per page; null means the default page size from Options.</summary>
    public int? PageSize { get; set; }
}
