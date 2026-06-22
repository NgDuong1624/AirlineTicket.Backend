namespace AirlineTicket.BuildingBlocks.Caching;

/// <summary>
/// Cache key and duration configuration for a single cache entry.
/// </summary>
public class CacheOptions
{
    public string Key { get; set; } = string.Empty;
    public TimeSpan AbsoluteExpirationRelativeToNow { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan? SlidingExpiration { get; set; }
}
