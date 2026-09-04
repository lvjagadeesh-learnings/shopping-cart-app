namespace BuildingBlocks.Common;

/// <summary>Standard paged result shape returned by every list endpoint across services.</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

/// <summary>Standard paging query parameters, bound from the query string.</summary>
public sealed class PagingQuery
{
    private const int MaxPageSize = 100;
    private int _pageSize = 20;

    public int Page { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value is > 0 and <= MaxPageSize ? value : _pageSize;
    }

    public int Skip => (Math.Max(Page, 1) - 1) * PageSize;
}
