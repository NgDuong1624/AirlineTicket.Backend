using System;
using System.Collections.Generic;

namespace AirlineTicket.Modules.Flights.Domain.Entities;

public class Route
{
    public Guid Id { get; set; }
    public Guid AirlineId { get; set; }
    public Guid OriginAirportId { get; set; }
    public Guid DestinationAirportId { get; set; }
    public decimal? DistanceKm { get; set; }
    public int? EstimatedDurationMinutes { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation properties
    public virtual Airline Airline { get; set; } = null!;
    public virtual Airport OriginAirport { get; set; } = null!;
    public virtual Airport DestinationAirport { get; set; } = null!;
    public virtual ICollection<Flight> Flights { get; set; } = new List<Flight>();
}
