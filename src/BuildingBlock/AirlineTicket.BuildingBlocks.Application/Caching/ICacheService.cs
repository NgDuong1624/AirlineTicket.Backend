namespace AirlineTicket.BuildingBlocks.Caching;

/// <summary>
/// Abstraction for caching operations.
/// In a monolithic setup, resolved to IDistributedCache backed by Redis.
/// In a future microservices split, this can be swapped for a distributed cache client without changing handlers.
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
    Task SetAsync<T>(string key, T value, CacheOptions? options = null, CancellationToken cancellationToken = default) where T : class;
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get or set via a factory function (cache-aside pattern).
    /// </summary>
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, CacheOptions? options = null, CancellationToken cancellationToken = default) where T : class;
}
