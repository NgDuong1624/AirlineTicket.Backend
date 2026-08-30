using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.Domain.Constants;

namespace AirlineTicket.SignalR.Hubs;

[Authorize]
public class NotificationHub : Hub<INotificationClient>
{
    public override async Task OnConnectedAsync()
    {
        // Add user to a group named after their UserId so NotificationPusher can target them
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? Context.User?.FindFirst(AuthConstants.Claims.Subject)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }

        var airlineId = Context.User?.FindFirst(AuthConstants.Claims.AirlineId)?.Value;
        if (!string.IsNullOrEmpty(airlineId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"airline-staff-{airlineId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(System.Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? Context.User?.FindFirst(AuthConstants.Claims.Subject)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
        }

        var airlineId = Context.User?.FindFirst(AuthConstants.Claims.AirlineId)?.Value;
        if (!string.IsNullOrEmpty(airlineId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"airline-staff-{airlineId}");
        }

        await base.OnDisconnectedAsync(exception);
    }
}