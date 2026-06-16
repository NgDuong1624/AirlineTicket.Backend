using System;
using AirlineTicket.Modules.Bookings.Domain.Enums;

namespace AirlineTicket.Modules.Bookings.Domain.Entities;

public class Ticket
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid PassengerId { get; set; }
    public Guid FlightId { get; set; }
    public Guid SeatId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string? Gate { get; set; }
    public DateTime? BoardingTime { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.Valid;

    // Navigation properties
    public virtual Booking Booking { get; set; } = null!;
    public virtual Passenger Passenger { get; set; } = null!;
}
