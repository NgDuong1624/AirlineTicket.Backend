using System.Collections.Concurrent;
using System.Text.Json;
using AirlineTicket.Modules.Flights.Application.Contracts;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace AirlineTicket.Modules.Flights.Infrastructure.Services;

public class RedisFlightTelemetryCache : IFlightTelemetryCache
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<RedisFlightTelemetryCache> _logger;
    private static readonly ConcurrentDictionary<Guid, (FlightTelemetryDto Data, DateTime ExpiresAt)> MemoryTelemetryCache = new();
    private static (List<AircraftMapPinDto> Planes, DateTime ExpiresAt)? MemoryRadarCache;

    public RedisFlightTelemetryCache(
        ILogger<RedisFlightTelemetryCache> logger,
        IConnectionMultiplexer? redis = null)
    {
        _logger = logger;
        _redis = redis;
    }

    private static string TelemetryKey(Guid flightId) => $"flight:telemetry:{flightId}";
    private const string ActiveRadarKey = "flight:telemetry:active";

    public async Task SetTelemetryAsync(
        Guid flightId,
        FlightTelemetryDto telemetry,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default)
    {
        var ttl = expiry ?? TimeSpan.FromMinutes(10);
        MemoryTelemetryCache[flightId] = (telemetry, DateTime.UtcNow.Add(ttl));

        if (_redis == null || !_redis.IsConnected) return;

        try
        {
            var db = _redis.GetDatabase();
            var json = JsonSerializer.Serialize(telemetry);
            await db.StringSetAsync(TelemetryKey(flightId), json, ttl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write telemetry to Redis for FlightId={FlightId}", flightId);
        }
    }

    public async Task<FlightTelemetryDto?> GetTelemetryAsync(
        Guid flightId,
        CancellationToken cancellationToken = default)
    {
        if (_redis != null && _redis.IsConnected)
        {
            try
            {
                var db = _redis.GetDatabase();
                var value = await db.StringGetAsync(TelemetryKey(flightId));
                if (value.HasValue && !value.IsNullOrEmpty)
                {
                    return JsonSerializer.Deserialize<FlightTelemetryDto>(value.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read telemetry from Redis for FlightId={FlightId}", flightId);
            }
        }

        if (MemoryTelemetryCache.TryGetValue(flightId, out var cached) && cached.ExpiresAt > DateTime.UtcNow)
        {
            return cached.Data;
        }

        return null;
    }

    public async Task SetActiveRadarPlanesAsync(
        List<AircraftMapPinDto> planes,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default)
    {
        var ttl = expiry ?? TimeSpan.FromSeconds(30);
        MemoryRadarCache = (planes, DateTime.UtcNow.Add(ttl));

        if (_redis == null || !_redis.IsConnected) return;

        try
        {
            var db = _redis.GetDatabase();
            var json = JsonSerializer.Serialize(planes);
            await db.StringSetAsync(ActiveRadarKey, json, ttl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write active radar planes to Redis");
        }
    }

    public async Task<List<AircraftMapPinDto>?> GetActiveRadarPlanesAsync(
        CancellationToken cancellationToken = default)
    {
        if (_redis != null && _redis.IsConnected)
        {
            try
            {
                var db = _redis.GetDatabase();
                var value = await db.StringGetAsync(ActiveRadarKey);
                if (value.HasValue && !value.IsNullOrEmpty)
                {
                    return JsonSerializer.Deserialize<List<AircraftMapPinDto>>(value.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read active radar planes from Redis");
            }
        }

        if (MemoryRadarCache.HasValue && MemoryRadarCache.Value.ExpiresAt > DateTime.UtcNow)
        {
            return MemoryRadarCache.Value.Planes;
        }

        return null;
    }
}
