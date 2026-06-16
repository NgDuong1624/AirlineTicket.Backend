using System;
using AirlineTicket.Modules.Flights.Domain.Enums;

namespace AirlineTicket.Modules.Flights.Domain.Entities;

public class FlightSeat
{
    public Guid Id { get; set; }
    public Guid FlightId { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public SeatClass SeatClass { get; set; }
    public decimal? PriceOverride { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsExtraLegroom { get; set; }

    // Navigation properties
    public virtual Flight Flight { get; set; } = null!;
}
