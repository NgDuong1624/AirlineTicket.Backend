using AirlineTicket.Modules.Bookings.Domain.Enums;

namespace AirlineTicket.Modules.Bookings.Domain.Entities;

public class Payment 
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public Booking Booking { get; set; } = null!;
}
