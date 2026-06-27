using System;
using AirlineTicket.Modules.Flights.Domain.Enums;

namespace AirlineTicket.Modules.Flights.Domain.Entities;

public class AircraftModelSeatTemplate
{
    public Guid Id { get; set; }
    public Guid AircraftModelId { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public string SeatRow { get; set; } = string.Empty;
    public string SeatColumn { get; set; } = string.Empty;
    public SeatClass SeatClass { get; set; }
    public bool IsExtraLegroom { get; set; }
    public decimal PriceMultiplier { get; set; } = 1.0m;

    // Navigation properties
    public virtual AircraftModel AircraftModel { get; set; } = null!;
}
