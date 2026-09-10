namespace AirlineTicket.Modules.Flights.Application.Contracts;

public class AdsbAircraftState
{
    public string Icao24 { get; set; } = string.Empty;
    public string Callsign { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int AltitudeFeet { get; set; }
    public int VelocityKnots { get; set; }
    public int TrueTrackDegrees { get; set; }
    public double VerticalRateFpm { get; set; }
    public string Squawk { get; set; } = "1200";
    public bool OnGround { get; set; }
    public short ProgressPercentage { get; set; }
    public DateTime LastContact { get; set; } = DateTime.UtcNow;
}

public interface IMockAdsbFeedService
{
    AdsbAircraftState ComputeFlightPosition(
        Guid flightId,
        string flightNumber,
        string airlineCode,
        double originLat,
        double originLon,
        double destLat,
        double destLon,
        DateTime scheduledDeparture,
        DateTime scheduledArrival,
        DateTime currentTime);
}
