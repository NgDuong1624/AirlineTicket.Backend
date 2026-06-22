using AirlineTicket.BuildingBlocks.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AirlineTicket.BuildingBlocks.Infrastructure.Caching;

/// <summary>
/// Distributed cache service backed by IDistributedCache (e.g., Redis).
/// Uses JSON serialization with camelCase for consistency.
/// In monolithic mode, works with any IDistributedCache provider.
/// In microservices mode, all services share the same Redis — cache keys are namespaced by module prefix.
/// </summary>
public class DistributedCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<DistributedCacheService> _logger;

    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        DateTimeZoneHandling = DateTimeZoneHandling.Utc
    };

    public DistributedCacheService(IDistributedCache cache, ILogger<DistributedCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var bytes = await _cache.GetAsync(key, cancellationToken);
            if (bytes is null) return null;

            var json = System.Text.Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(json, JsonSettings);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache GET failed for key {Key}. Returning default.", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, CacheOptions? options = null, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var json = JsonConvert.SerializeObject(value, JsonSettings);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);

            var distributedOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = options?.AbsoluteExpirationRelativeToNow ?? TimeSpan.FromMinutes(5),
                SlidingExpiration = options?.SlidingExpiration
            };

            await _cache.SetAsync(key, bytes, distributedOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache SET failed for key {Key}.", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache REMOVE failed for key {Key}.", key);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var value = await _cache.GetAsync(key, cancellationToken);
            return value is not null;
        }
        catch
        {
            return false;
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, CacheOptions? options = null, CancellationToken cancellationToken = default) where T : class
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var value = await factory();
        if (value is not null)
        {
            await SetAsync(key, value, options, cancellationToken);
        }

        return value!;
    }
}
