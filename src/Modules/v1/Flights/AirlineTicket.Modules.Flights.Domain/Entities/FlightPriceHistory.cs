using System;

namespace AirlineTicket.Modules.Flights.Domain.Entities;

public class FlightPriceHistory
{
    public Guid Id { get; set; }
    public Guid FlightId { get; set; }
    public Guid RouteId { get; set; }
    public decimal Price { get; set; }
    public string SeatClass { get; set; } = "Economy";
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Flight Flight { get; set; } = null!;
    public virtual Route Route { get; set; } = null!;
}