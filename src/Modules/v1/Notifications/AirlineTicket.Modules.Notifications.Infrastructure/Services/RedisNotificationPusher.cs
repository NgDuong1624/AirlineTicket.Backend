using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Notifications.Application.Contracts;
using AirlineTicket.Modules.Notifications.Application.DTOs;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace AirlineTicket.Modules.Notifications.Infrastructure.Services;

public record NotificationPubSubMessage(
    string TargetType, // "User" or "AirlineStaff"
    Guid? UserId,
    Guid? AirlineId,
    NotificationDto Notification);

public class RedisNotificationPusher : INotificationPusher
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisNotificationPusher> _logger;
    public const string ChannelName = "AirlineTicket:Notifications";

    public RedisNotificationPusher(IConnectionMultiplexer redis, ILogger<RedisNotificationPusher> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task PushNotificationAsync(Guid userId, NotificationDto notification, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new NotificationPubSubMessage("User", userId, null, notification);
            var payload = JsonSerializer.Serialize(message);
            var subscriber = _redis.GetSubscriber();
            await subscriber.PublishAsync(RedisChannel.Literal(ChannelName), payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish notification to Redis for UserId={UserId}", userId);
        }
    }

    public async Task PushToAirlineStaffAsync(Guid airlineId, NotificationDto notification, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new NotificationPubSubMessage("AirlineStaff", null, airlineId, notification);
            var payload = JsonSerializer.Serialize(message);
            var subscriber = _redis.GetSubscriber();
            await subscriber.PublishAsync(RedisChannel.Literal(ChannelName), payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish notification to Redis for AirlineId={AirlineId}", airlineId);
        }
    }
}
