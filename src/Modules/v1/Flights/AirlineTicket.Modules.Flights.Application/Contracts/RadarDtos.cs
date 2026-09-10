namespace AirlineTicket.Modules.Flights.Application.Contracts;

public class FlightTelemetryDto
{
    public Guid FlightId { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public string AirlineCode { get; set; } = string.Empty;
    public string AirlineName { get; set; } = string.Empty;
    public string OriginCode { get; set; } = string.Empty;
    public string DestinationCode { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int AltitudeFeet { get; set; }
    public int SpeedKnots { get; set; }
    public int HeadingDegrees { get; set; }
    public short ProgressPercentage { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? DepartureGate { get; set; }
    public string? ArrivalGate { get; set; }
    public string? BaggageCarousel { get; set; }
    public int DelayMinutes { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public DateTime EstimatedArrival { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}

public class AircraftMapPinDto
{
    public Guid FlightId { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public string AirlineCode { get; set; } = string.Empty;
    public string AirlineName { get; set; } = string.Empty;
    public string OriginCode { get; set; } = string.Empty;
    public string DestinationCode { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int AltitudeFeet { get; set; }
    public int SpeedKnots { get; set; }
    public int HeadingDegrees { get; set; }
    public short ProgressPercentage { get; set; }
    public string Status { get; set; } = string.Empty;
    public int DelayMinutes { get; set; }
}

public class FlightStatusChangedDto
{
    public Guid FlightId { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string? DepartureGate { get; set; }
    public string? ArrivalGate { get; set; }
    public string? BaggageCarousel { get; set; }
    public int DelayMinutes { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class FlightStatusDetailDto
{
    public Guid FlightId { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public string AirlineCode { get; set; } = string.Empty;
    public string AirlineName { get; set; } = string.Empty;
    public string OriginCode { get; set; } = string.Empty;
    public string OriginName { get; set; } = string.Empty;
    public string OriginCity { get; set; } = string.Empty;
    public double OriginLatitude { get; set; }
    public double OriginLongitude { get; set; }
    public string DestinationCode { get; set; } = string.Empty;
    public string DestinationName { get; set; } = string.Empty;
    public string DestinationCity { get; set; } = string.Empty;
    public double DestinationLatitude { get; set; }
    public double DestinationLongitude { get; set; }
    public DateTime ScheduledDeparture { get; set; }
    public DateTime? ActualDeparture { get; set; }
    public DateTime ScheduledArrival { get; set; }
    public DateTime? ActualArrival { get; set; }
    public DateTime EstimatedArrival { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? DepartureGate { get; set; }
    public string? ArrivalGate { get; set; }
    public string? BaggageCarousel { get; set; }
    public int DelayMinutes { get; set; }
    public FlightTelemetryDto? CurrentTelemetry { get; set; }
    public List<FlightTelemetryDto> RouteHistory { get; set; } = new();
}

public class FlightRadarPubSubMessage
{
    public const string ChannelName = "flight-radar-updates";

    public string EventType { get; set; } = string.Empty;
    public Guid? FlightId { get; set; }
    public FlightTelemetryDto? Telemetry { get; set; }
    public List<AircraftMapPinDto>? ActivePlanes { get; set; }
    public FlightStatusChangedDto? StatusChanged { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
