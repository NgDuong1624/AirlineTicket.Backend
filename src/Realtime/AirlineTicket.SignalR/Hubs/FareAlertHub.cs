using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AirlineTicket.SignalR.Hubs;

[Authorize]
public class FareAlertHub : Hub<IFareAlertClient>
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? Context.User?.FindFirst("sub")?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? Context.User?.FindFirst("sub")?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SubscribeRoute(string originAirportId, string destinationAirportId, string departureDate)
    {
        var groupName = $"route_{originAirportId}_{destinationAirportId}_{departureDate}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task UnsubscribeRoute(string originAirportId, string destinationAirportId, string departureDate)
    {
        var groupName = $"route_{originAirportId}_{destinationAirportId}_{departureDate}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}
