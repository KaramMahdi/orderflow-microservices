using Domain.Shared.Layer.Dtos;

namespace OrderFlow.Domain.Tests.SharedLayer;

/// <summary>
/// The generic paging DTOs (ADR-0006): a page of items with the total count across all pages,
/// and a request that starts on the first page with no explicit page size.
/// </summary>
public sealed class PagingDtoTests
{
    [Fact]
    public void PagedResultCarriesTheItemsAndTheTotalCount()
    {
        string[] items = ["a", "b", "c"];
        const int totalCount = 25;

        var result = new PagedResultDto<string>(items, totalCount);

        Assert.Equal(items, result.Items);
        Assert.Equal(totalCount, result.TotalCount);
    }

    [Fact]
    public void PagedResultTotalCountCanExceedThePageSize()
    {
        var result = new PagedResultDto<int>([1, 2], 1000);

        Assert.True(result.TotalCount > result.Items.Count);
    }

    [Fact]
    public void PagedResultCanBeAnEmptyPage()
    {
        var result = new PagedResultDto<int>([], 0);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public void PagedResultsWithTheSameListAndCountAreEqual()
    {
        IReadOnlyList<int> items = [1, 2, 3];

        Assert.Equal(new PagedResultDto<int>(items, 3), new PagedResultDto<int>(items, 3));
        Assert.NotEqual(new PagedResultDto<int>(items, 3), new PagedResultDto<int>(items, 4));
    }

    [Fact]
    public void PagedRequestStartsOnTheFirstPageWithTheDefaultPageSize()
    {
        var request = new PagedRequestDto();

        Assert.Equal(PagedRequestDto.FirstPageNumber, request.PageNumber);
        Assert.Equal(1, PagedRequestDto.FirstPageNumber);
        Assert.Null(request.PageSize);
    }
}
