namespace AirlineTicket.BuildingBlocks.Responses;

public class PagedResult<TValue> : Result
{
    public IReadOnlyCollection<TValue> Items { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;

    [Newtonsoft.Json.JsonConstructor]
    [System.Text.Json.Serialization.JsonConstructor]
    public PagedResult(
        IReadOnlyCollection<TValue> items,
        int pageNumber,
        int pageSize,
        int totalCount,
        bool isSuccess,
        Error error)
        : base(isSuccess, error)
    {
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public static PagedResult<TValue> Success(
        IReadOnlyCollection<TValue> items,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        return new PagedResult<TValue>(items, pageNumber, pageSize, totalCount, true, Error.None);
    }

    public static new PagedResult<TValue> Failure(Error error)
    {
        return new PagedResult<TValue>([], 0, 0, 0, false, error);
    }
}
