using System.Threading.Tasks;
using AirlineTicket.Modules.Notifications.Application.DTOs;

namespace AirlineTicket.SignalR.Hubs;

public interface INotificationClient
{
    Task ReceiveNotification(NotificationDto notification);
}
