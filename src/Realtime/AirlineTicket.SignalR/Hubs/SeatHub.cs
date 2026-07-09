using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace AirlineTicket.SignalR.Hubs;

/// <summary>
/// Realtime channel for the staff seat map. Clients join a per-flight group and receive
/// <c>SeatUpdated</c> events whenever a seat's availability changes.
/// </summary>
public class SeatHub : Hub
{
    public static string GroupFor(Guid flightId) => $"flight-{flightId}";

    public Task JoinFlight(Guid flightId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, GroupFor(flightId));

    public Task LeaveFlight(Guid flightId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupFor(flightId));
}
