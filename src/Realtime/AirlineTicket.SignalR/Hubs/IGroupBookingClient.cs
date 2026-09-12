using System.Threading.Tasks;

namespace AirlineTicket.SignalR.Hubs;

public interface IGroupBookingClient
{
    Task MemberJoined(object member);
    Task MemberSeatChanged(string memberId, string seatNumber, bool isReturn);
    Task MemberPaymentCompleted(string memberId, decimal amountPaid, decimal remainingAmount);
    Task GroupBookingCompleted(string inviteCode);
    Task GroupExpired();
    Task UserPresenceChanged(string memberId, string passengerName, bool isOnline);
}
