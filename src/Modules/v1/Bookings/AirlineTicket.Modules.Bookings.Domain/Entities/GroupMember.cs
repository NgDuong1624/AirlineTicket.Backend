using System;
using AirlineTicket.Modules.Bookings.Domain.Enums;

namespace AirlineTicket.Modules.Bookings.Domain.Entities;

public class GroupMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GroupBookingId { get; set; }
    public Guid? UserId { get; set; }
    public string PassengerName { get; set; } = string.Empty;
    public string PassengerEmail { get; set; } = string.Empty;
    public string? PassengerPhone { get; set; }
    public string? SeatNumber { get; set; }
    public string? ReturnSeatNumber { get; set; }
    public decimal AssignedAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public MemberPaymentStatus PaymentStatus { get; set; } = MemberPaymentStatus.Pending;
    public string? PaymentTransactionId { get; set; }
    public PaymentProvider? PaymentProvider { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual GroupBooking GroupBooking { get; set; } = null!;
}
