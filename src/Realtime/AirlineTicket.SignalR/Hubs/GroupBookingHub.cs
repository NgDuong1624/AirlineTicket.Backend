using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace AirlineTicket.SignalR.Hubs;

public class GroupBookingHub : Hub<IGroupBookingClient>
{
    public static string GroupFor(string inviteCode) => $"group-{inviteCode.Trim().ToUpperInvariant()}";

    public async Task JoinGroupRoom(string inviteCode, string memberId, string passengerName)
    {
        var groupName = GroupFor(inviteCode);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await Clients.OthersInGroup(groupName).UserPresenceChanged(memberId, passengerName, true);
    }

    public async Task LeaveGroupRoom(string inviteCode, string memberId, string passengerName)
    {
        var groupName = GroupFor(inviteCode);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        await Clients.OthersInGroup(groupName).UserPresenceChanged(memberId, passengerName, false);
    }

    public async Task BroadcastSeatChange(string inviteCode, string memberId, string seatNumber, bool isReturn)
    {
        var groupName = GroupFor(inviteCode);
        await Clients.OthersInGroup(groupName).MemberSeatChanged(memberId, seatNumber, isReturn);
    }
}
