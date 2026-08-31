using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Notifications.Application.DTOs;
using AirlineTicket.Modules.Notifications.Infrastructure.Services;
using AirlineTicket.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace AirlineTicket.SignalR.Services;

public class NotificationRedisSubscriber : IHostedService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;
    private readonly ILogger<NotificationRedisSubscriber> _logger;

    public NotificationRedisSubscriber(
        IConnectionMultiplexer redis,
        IHubContext<NotificationHub, INotificationClient> hubContext,
        ILogger<NotificationRedisSubscriber> logger)
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
                RedisChannel.Literal(RedisNotificationPusher.ChannelName),
                async (channel, message) =>
                {
                    if (message.IsNullOrEmpty) return;

                    try
                    {
                        var payload = JsonSerializer.Deserialize<NotificationPubSubMessage>(message.ToString());
                        if (payload?.Notification == null) return;

                        if (payload.TargetType == "User" && payload.UserId.HasValue)
                        {
                            await _hubContext.Clients.Group(payload.UserId.Value.ToString()).ReceiveNotification(payload.Notification);
                        }
                        else if (payload.TargetType == "AirlineStaff" && payload.AirlineId.HasValue)
                        {
                            await _hubContext.Clients.Group($"airline-staff-{payload.AirlineId.Value}").ReceiveNotification(payload.Notification);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing received notification message from Redis Pub/Sub");
                    }
                });

            _logger.LogInformation("Subscribed to Redis channel: {Channel}", RedisNotificationPusher.ChannelName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe to Redis notifications channel");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            var subscriber = _redis.GetSubscriber();
            await subscriber.UnsubscribeAsync(RedisChannel.Literal(RedisNotificationPusher.ChannelName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing from Redis notifications channel");
        }
    }
}
