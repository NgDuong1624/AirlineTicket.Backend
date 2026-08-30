using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Notifications.Application.DTOs;

namespace AirlineTicket.Modules.Notifications.Application.Contracts;

public interface INotificationPusher
{
    Task PushNotificationAsync(Guid userId, NotificationDto notification, CancellationToken cancellationToken = default);
    Task PushToAirlineStaffAsync(Guid airlineId, NotificationDto notification, CancellationToken cancellationToken = default);
}
