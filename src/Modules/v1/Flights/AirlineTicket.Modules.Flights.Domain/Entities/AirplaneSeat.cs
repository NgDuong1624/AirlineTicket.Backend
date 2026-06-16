using System;

namespace AirlineTicket.Modules.Flights.Domain.Entities;

public class AirplaneSeat
{
    public Guid Id { get; set; }
    public Guid AirplaneId { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public string SeatRow { get; set; } = string.Empty;
    public string SeatColumn { get; set; } = string.Empty;
    public bool IsExtraLegroom { get; set; }
    public decimal PriceMultiplier { get; set; } = 1.0m;

    // Navigation properties
    public virtual Airplane Airplane { get; set; } = null!;
}