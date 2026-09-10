using Microsoft.AspNetCore.SignalR;

namespace AirlineTicket.SignalR.Hubs;

public class FlightTrackerHub : Hub<IFlightTrackerClient>
{
    public const string GlobalRadarGroup = "global-sky-radar";

    public static string FlightRoom(Guid flightId) => $"flight-telemetry-{flightId}";

    public async Task JoinFlightTracking(Guid flightId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, FlightRoom(flightId));
    }

    public async Task LeaveFlightTracking(Guid flightId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, FlightRoom(flightId));
    }

    public async Task JoinGlobalRadar()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GlobalRadarGroup);
    }

    public async Task LeaveGlobalRadar()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GlobalRadarGroup);
    }
}
