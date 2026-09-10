using System.Text.Json;
using AirlineTicket.Modules.Flights.Application.Contracts;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace AirlineTicket.Modules.Flights.Infrastructure.Services;

public class RedisFlightRadarPublisher : IFlightRadarPublisher
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<RedisFlightRadarPublisher> _logger;

    public RedisFlightRadarPublisher(
        ILogger<RedisFlightRadarPublisher> logger,
        IConnectionMultiplexer? redis = null)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task PublishTelemetryUpdateAsync(FlightTelemetryDto telemetry, CancellationToken cancellationToken = default)
    {
        if (_redis == null || !_redis.IsConnected)
        {
            _logger.LogDebug("Redis multiplexer not connected. Skipping telemetry pub/sub broadcast for FlightId={FlightId}", telemetry.FlightId);
            return;
        }

        try
        {
            var message = new FlightRadarPubSubMessage
            {
                EventType = "TelemetryUpdated",
                FlightId = telemetry.FlightId,
                Telemetry = telemetry,
                Timestamp = DateTime.UtcNow
            };

            var payload = JsonSerializer.Serialize(message);
            var subscriber = _redis.GetSubscriber();
            await subscriber.PublishAsync(RedisChannel.Literal(FlightRadarPubSubMessage.ChannelName), payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish TelemetryUpdated to Redis for FlightId={FlightId}", telemetry.FlightId);
        }
    }

    public async Task PublishGlobalRadarTickAsync(List<AircraftMapPinDto> activePlanes, CancellationToken cancellationToken = default)
    {
        if (_redis == null || !_redis.IsConnected)
        {
            _logger.LogDebug("Redis multiplexer not connected. Skipping global radar tick pub/sub broadcast");
            return;
        }

        try
        {
            var message = new FlightRadarPubSubMessage
            {
                EventType = "GlobalRadarTick",
                ActivePlanes = activePlanes,
                Timestamp = DateTime.UtcNow
            };

            var payload = JsonSerializer.Serialize(message);
            var subscriber = _redis.GetSubscriber();
            await subscriber.PublishAsync(RedisChannel.Literal(FlightRadarPubSubMessage.ChannelName), payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish GlobalRadarTick to Redis for {Count} planes", activePlanes.Count);
        }
    }

    public async Task PublishStatusChangeAsync(FlightStatusChangedDto statusChanged, CancellationToken cancellationToken = default)
    {
        if (_redis == null || !_redis.IsConnected)
        {
            _logger.LogDebug("Redis multiplexer not connected. Skipping status change pub/sub broadcast for FlightId={FlightId}", statusChanged.FlightId);
            return;
        }

        try
        {
            var message = new FlightRadarPubSubMessage
            {
                EventType = "FlightStatusChanged",
                FlightId = statusChanged.FlightId,
                StatusChanged = statusChanged,
                Timestamp = DateTime.UtcNow
            };

            var payload = JsonSerializer.Serialize(message);
            var subscriber = _redis.GetSubscriber();
            await subscriber.PublishAsync(RedisChannel.Literal(FlightRadarPubSubMessage.ChannelName), payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish FlightStatusChanged to Redis for FlightId={FlightId}", statusChanged.FlightId);
        }
    }
}
