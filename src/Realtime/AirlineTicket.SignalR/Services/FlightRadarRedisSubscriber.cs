using System.Text.Json;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace AirlineTicket.SignalR.Services;

public class FlightRadarRedisSubscriber : IHostedService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IHubContext<FlightTrackerHub, IFlightTrackerClient> _hubContext;
    private readonly ILogger<FlightRadarRedisSubscriber> _logger;

    public FlightRadarRedisSubscriber(
        IConnectionMultiplexer redis,
        IHubContext<FlightTrackerHub, IFlightTrackerClient> hubContext,
        ILogger<FlightRadarRedisSubscriber> logger)
    {
        _redis = redis;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var subscriber = _redis.GetSubscriber();
            await subscriber.SubscribeAsync(
                RedisChannel.Literal(FlightRadarPubSubMessage.ChannelName),
                async (channel, message) =>
                {
                    if (message.IsNullOrEmpty) return;

                    try
                    {
                        var payload = JsonSerializer.Deserialize<FlightRadarPubSubMessage>(message.ToString());
                        if (payload == null) return;

                        if (payload.EventType == "TelemetryUpdated" && payload.Telemetry != null && payload.FlightId.HasValue)
                        {
                            var room = FlightTrackerHub.FlightRoom(payload.FlightId.Value);
                            await _hubContext.Clients.Group(room).TelemetryUpdated(payload.Telemetry);
                        }
                        else if (payload.EventType == "GlobalRadarTick" && payload.ActivePlanes != null)
                        {
                            await _hubContext.Clients.Group(FlightTrackerHub.GlobalRadarGroup).GlobalRadarTick(payload.ActivePlanes);
                        }
                        else if (payload.EventType == "FlightStatusChanged" && payload.StatusChanged != null && payload.FlightId.HasValue)
                        {
                            var room = FlightTrackerHub.FlightRoom(payload.FlightId.Value);
                            await _hubContext.Clients.Group(room).FlightStatusChanged(payload.StatusChanged);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing flight radar pub/sub message");
                    }
                });

            _logger.LogInformation("Subscribed to Redis channel: {Channel}", FlightRadarPubSubMessage.ChannelName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe to Redis flight radar channel");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            var subscriber = _redis.GetSubscriber();
            await subscriber.UnsubscribeAsync(RedisChannel.Literal(FlightRadarPubSubMessage.ChannelName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing from Redis flight radar channel");
        }
    }
}
