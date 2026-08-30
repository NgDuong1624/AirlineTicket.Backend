using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using AirlineTicket.Modules.Notifications.Application.Contracts;
using AirlineTicket.Modules.Notifications.Application.DTOs;
using AirlineTicket.SignalR.Hubs;

namespace AirlineTicket.SignalR.Services;

public class NotificationPusher : INotificationPusher
{
    private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;

    public NotificationPusher(IHubContext<NotificationHub, INotificationClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task PushNotificationAsync(Guid userId, NotificationDto notification, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group(userId.ToString()).ReceiveNotification(notification);
    }

    public async Task PushToAirlineStaffAsync(Guid airlineId, NotificationDto notification, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group($"airline-staff-{airlineId}").ReceiveNotification(notification);
    }
}
