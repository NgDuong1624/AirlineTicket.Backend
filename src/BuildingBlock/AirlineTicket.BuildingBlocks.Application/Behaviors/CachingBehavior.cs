using AirlineTicket.BuildingBlocks.Caching;
using MediatR;

namespace AirlineTicket.BuildingBlocks.Behaviors;

/// <summary>
/// Marker interface to indicate a query (or any request) should be cached.
/// Implement this on queries that return read-only, cacheable data.
/// </summary>
public interface ICacheableRequest
{
    /// <summary>
    /// Unique cache key for this request instance.
    /// Typically built from request parameters.
    /// </summary>
    string CacheKey { get; }

    /// <summary>
    /// Duration in minutes for the cache entry. Default 5.
    /// </summary>
    int CacheDurationMinutes => 5;
}

/// <summary>
/// MediatR pipeline behavior that caches responses for requests marked with ICacheableRequest.
/// Skips caching if the response represents a failure.
/// </summary>
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : class
{
    private readonly ICacheService _cacheService;

    public CachingBehavior(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Only cache requests that implement ICacheableRequest
        if (request is not ICacheableRequest cacheableRequest)
        {
            return await next();
        }

        var cacheKey = cacheableRequest.CacheKey;
        var cached = await _cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var response = await next();

        // Only cache successful responses
        // (We check via the Result pattern — if the response has IsSuccess=false, skip caching)
        var isSuccess = IsSuccessResponse(response);
        if (isSuccess)
        {
            var options = new CacheOptions
            {
                Key = cacheKey,
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheableRequest.CacheDurationMinutes)
            };
            await _cacheService.SetAsync(cacheKey, response, options, cancellationToken);
        }

        return response;
    }

    private static bool IsSuccessResponse(TResponse response)
    {
        // Duck-type check: if the response type has an IsSuccess property, respect it
        var prop = typeof(TResponse).GetProperty("IsSuccess");
        if (prop is not null && prop.PropertyType == typeof(bool))
        {
            return (bool)prop.GetValue(response)!;
        }

        // Default: cache the response (optimistic)
        return true;
    }
}
