using System.Security.Cryptography;
using System.Text;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Common;

namespace AirlineTicket.Modules.Flights.Infrastructure.Services;

public class MockAdsbFeedService : IMockAdsbFeedService
{
    public AdsbAircraftState ComputeFlightPosition(
        Guid flightId,
        string flightNumber,
        string airlineCode,
        double originLat,
        double originLon,
        double destLat,
        double destLon,
        DateTime scheduledDeparture,
        DateTime scheduledArrival,
        DateTime currentTime)
    {
        var icao24 = GenerateIcao24(flightNumber);
        var callsign = string.IsNullOrWhiteSpace(airlineCode) ? flightNumber : $"{airlineCode}{flightNumber.TrimStart(airlineCode.ToCharArray())}";

        var totalSeconds = (scheduledArrival - scheduledDeparture).TotalSeconds;
        if (totalSeconds <= 0)
        {
            totalSeconds = 3600; // default 1 hour fallback
        }

        var elapsedSeconds = (currentTime - scheduledDeparture).TotalSeconds;
        double progressFraction = Math.Clamp(elapsedSeconds / totalSeconds, 0.0, 1.0);
        short progressPercentage = (short)Math.Round(progressFraction * 100.0);

        if (progressFraction <= 0.0)
        {
            // At origin airport gate/tarmac
            var initialBearing = GeoMath.CalculateBearing(originLat, originLon, destLat, destLon);
            return new AdsbAircraftState
            {
                Icao24 = icao24,
                Callsign = callsign,
                Latitude = originLat,
                Longitude = originLon,
                AltitudeFeet = 0,
                VelocityKnots = 0,
                TrueTrackDegrees = initialBearing,
                VerticalRateFpm = 0,
                Squawk = "1200",
                OnGround = true,
                ProgressPercentage = 0,
                LastContact = currentTime
            };
        }

        if (progressFraction >= 1.0)
        {
            // Landed at destination airport
            var finalBearing = GeoMath.CalculateBearing(originLat, originLon, destLat, destLon);
            return new AdsbAircraftState
            {
                Icao24 = icao24,
                Callsign = callsign,
                Latitude = destLat,
                Longitude = destLon,
                AltitudeFeet = 0,
                VelocityKnots = 0,
                TrueTrackDegrees = finalBearing,
                VerticalRateFpm = 0,
                Squawk = "1200",
                OnGround = true,
                ProgressPercentage = 100,
                LastContact = currentTime
            };
        }

        // Airborne: calculate Great-Circle position and heading
        var (currentLat, currentLon) = GeoMath.InterpolatePosition(originLat, originLon, destLat, destLon, progressFraction);
        var bearing = GeoMath.CalculateBearing(currentLat, currentLon, destLat, destLon);
        var altitude = GeoMath.EstimateAltitudeFeet(progressFraction);
        var speed = GeoMath.EstimateSpeedKnots(progressFraction);

        double verticalRate = 0;
        if (progressFraction < 0.15)
        {
            verticalRate = 2200; // Climbing
        }
        else if (progressFraction > 0.85)
        {
            verticalRate = -1800; // Descending
        }

        return new AdsbAircraftState
        {
            Icao24 = icao24,
            Callsign = callsign,
            Latitude = currentLat,
            Longitude = currentLon,
            AltitudeFeet = altitude,
            VelocityKnots = speed,
            TrueTrackDegrees = bearing,
            VerticalRateFpm = verticalRate,
            Squawk = GenerateSquawk(flightNumber),
            OnGround = false,
            ProgressPercentage = progressPercentage,
            LastContact = currentTime
        };
    }

    private static string GenerateIcao24(string flightNumber)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(flightNumber));
        return Convert.ToHexString(bytes[..3]).ToLowerInvariant();
    }

    private static string GenerateSquawk(string flightNumber)
    {
        int hash = Math.Abs(flightNumber.GetHashCode());
        int digit1 = (hash % 7) + 1;
        int digit2 = ((hash / 10) % 7);
        int digit3 = ((hash / 100) % 7);
        int digit4 = ((hash / 1000) % 7);
        return $"{digit1}{digit2}{digit3}{digit4}";
    }
}
