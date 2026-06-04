using AirlineTicket.Modules.Bookings.Domain.Enums;

namespace AirlineTicket.Modules.Bookings.Domain.Entities;

public class Booking 
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string PnrCode { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<Passenger> Passengers { get; set; } = new List<Passenger>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
