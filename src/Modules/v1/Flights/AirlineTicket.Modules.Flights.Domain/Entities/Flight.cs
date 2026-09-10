using AirlineTicket.Modules.Flights.Domain.Enums;

namespace AirlineTicket.Modules.Flights.Domain.Entities;

public class Flight
{
    public Guid Id { get; set; }
    public Guid RouteId { get; set; }
    public Guid AirplaneId { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public decimal BasePrice { get; set; }
    public string Currency { get; set; } = "USD";
    public FlightStatus Status { get; set; } = FlightStatus.Scheduled;
    public string? ExternalId { get; set; }
    public string? DepartureGate { get; set; }
    public string? ArrivalGate { get; set; }
    public string? BaggageCarousel { get; set; }
    public DateTime? ActualDepartureTime { get; set; }
    public DateTime? ActualArrivalTime { get; set; }
    public int DelayMinutes { get; set; } = 0;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Route Route { get; set; } = null!;
    public virtual Airplane Airplane { get; set; } = null!;
    public virtual ICollection<FlightSeat> FlightSeats { get; set; } = new List<FlightSeat>();
    public virtual ICollection<FlightTelemetry> Telemetries { get; set; } = new List<FlightTelemetry>();
}
