using System.Threading.Tasks;

namespace AirlineTicket.Modules.Bookings.Application.Contracts;

public interface IGroupBookingRealtimeNotifier
{
    Task NotifyMemberJoinedAsync(string inviteCode, object member);
    Task NotifySeatChangedAsync(string inviteCode, string memberId, string seatNumber, bool isReturn);
    Task NotifyPaymentCompletedAsync(string inviteCode, string memberId, decimal amountPaid, decimal remainingAmount);
    Task NotifyGroupCompletedAsync(string inviteCode);
    Task NotifyGroupExpiredAsync(string inviteCode);
}
