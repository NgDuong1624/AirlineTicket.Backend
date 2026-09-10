namespace AirlineTicket.Modules.Flights.Domain.Entities;

public class FlightTelemetry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FlightId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int AltitudeFeet { get; set; }
    public int SpeedKnots { get; set; }
    public int HeadingDegrees { get; set; }
    public short ProgressPercentage { get; set; }
    public DateTime EstimatedArrival { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual Flight Flight { get; set; } = null!;
}
