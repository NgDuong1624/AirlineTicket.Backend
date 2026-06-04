using AirlineTicket.Modules.Bookings.Domain.Enums;

namespace AirlineTicket.Modules.Bookings.Domain.Entities;

public class Ticket 
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid FlightId { get; set; }
    public Guid PassengerId { get; set; }
    public Guid SeatId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public TicketStatus Status { get; set; }

    public Booking Booking { get; set; } = null!;
    public Passenger Passenger { get; set; } = null!;
}
