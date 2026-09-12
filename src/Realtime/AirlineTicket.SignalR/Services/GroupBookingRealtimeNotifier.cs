using System.Threading.Tasks;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace AirlineTicket.SignalR.Services;

public class GroupBookingRealtimeNotifier : IGroupBookingRealtimeNotifier
{
    private readonly IHubContext<GroupBookingHub, IGroupBookingClient> _hubContext;

    public GroupBookingRealtimeNotifier(IHubContext<GroupBookingHub, IGroupBookingClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyMemberJoinedAsync(string inviteCode, object member)
    {
        var groupName = GroupBookingHub.GroupFor(inviteCode);
        await _hubContext.Clients.Group(groupName).MemberJoined(member);
    }

    public async Task NotifySeatChangedAsync(string inviteCode, string memberId, string seatNumber, bool isReturn)
    {
        var groupName = GroupBookingHub.GroupFor(inviteCode);
        await _hubContext.Clients.Group(groupName).MemberSeatChanged(memberId, seatNumber, isReturn);
    }

    public async Task NotifyPaymentCompletedAsync(string inviteCode, string memberId, decimal amountPaid, decimal remainingAmount)
    {
        var groupName = GroupBookingHub.GroupFor(inviteCode);
        await _hubContext.Clients.Group(groupName).MemberPaymentCompleted(memberId, amountPaid, remainingAmount);
    }

    public async Task NotifyGroupCompletedAsync(string inviteCode)
    {
        var groupName = GroupBookingHub.GroupFor(inviteCode);
        await _hubContext.Clients.Group(groupName).GroupBookingCompleted(inviteCode);
    }

    public async Task NotifyGroupExpiredAsync(string inviteCode)
    {
        var groupName = GroupBookingHub.GroupFor(inviteCode);
        await _hubContext.Clients.Group(groupName).GroupExpired();
    }
}
