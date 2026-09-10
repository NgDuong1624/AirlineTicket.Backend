namespace AirlineTicket.Modules.Flights.Domain.Common;

public static class GeoMath
{
    public const double EarthRadiusKm = 6371.0;
    public const double EarthRadiusNauticalMiles = 3440.065;
    public const double KmToNauticalMiles = 0.539957;

    public static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
    public static double ToDegrees(double radians) => radians * 180.0 / Math.PI;

    /// <summary>
    /// Calculates Great-Circle distance in kilometers between two coordinates using Haversine formula.
    /// </summary>
    public static double HaversineDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        double dLat = ToRadians(lat2 - lat1);
        double dLon = ToRadians(lon2 - lon1);

        double rLat1 = ToRadians(lat1);
        double rLat2 = ToRadians(lat2);

        double a = Math.Sin(dLat / 2.0) * Math.Sin(dLat / 2.0) +
                   Math.Cos(rLat1) * Math.Cos(rLat2) *
                   Math.Sin(dLon / 2.0) * Math.Sin(dLon / 2.0);

        double c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));
        return EarthRadiusKm * c;
    }

    /// <summary>
    /// Calculates initial bearing (forward azimuth) from point 1 to point 2 in degrees [0, 360).
    /// </summary>
    public static int CalculateBearing(double lat1, double lon1, double lat2, double lon2)
    {
        double rLat1 = ToRadians(lat1);
        double rLat2 = ToRadians(lat2);
        double dLon = ToRadians(lon2 - lon1);

        double y = Math.Sin(dLon) * Math.Cos(rLat2);
        double x = Math.Cos(rLat1) * Math.Sin(rLat2) -
                   Math.Sin(rLat1) * Math.Cos(rLat2) * Math.Cos(dLon);

        double bearing = (ToDegrees(Math.Atan2(y, x)) + 360.0) % 360.0;
        return (int)Math.Round(bearing) % 360;
    }

    /// <summary>
    /// Computes intermediate position along Great-Circle path at given fraction [0.0, 1.0].
    /// </summary>
    public static (double Latitude, double Longitude) InterpolatePosition(
        double lat1, double lon1, double lat2, double lon2, double fraction)
    {
        if (fraction <= 0.0) return (lat1, lon1);
        if (fraction >= 1.0) return (lat2, lon2);

        double rLat1 = ToRadians(lat1);
        double rLon1 = ToRadians(lon1);
        double rLat2 = ToRadians(lat2);
        double rLon2 = ToRadians(lon2);

        double dLat = rLat2 - rLat1;
        double dLon = rLon2 - rLon1;

        double a = Math.Sin(dLat / 2.0) * Math.Sin(dLat / 2.0) +
                   Math.Cos(rLat1) * Math.Cos(rLat2) *
                   Math.Sin(dLon / 2.0) * Math.Sin(dLon / 2.0);

        double d = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));

        if (d < 1e-7)
        {
            return (lat1, lon1);
        }

        double aRatio = Math.Sin((1.0 - fraction) * d) / Math.Sin(d);
        double bRatio = Math.Sin(fraction * d) / Math.Sin(d);

        double x = aRatio * Math.Cos(rLat1) * Math.Cos(rLon1) + bRatio * Math.Cos(rLat2) * Math.Cos(rLon2);
        double y = aRatio * Math.Cos(rLat1) * Math.Sin(rLon1) + bRatio * Math.Cos(rLat2) * Math.Sin(rLon2);
        double z = aRatio * Math.Sin(rLat1) + bRatio * Math.Sin(rLat2);

        double lat = Math.Atan2(z, Math.Sqrt(x * x + y * y));
        double lon = Math.Atan2(y, x);

        double latDeg = Math.Round(ToDegrees(lat), 6);
        double lonDeg = Math.Round((ToDegrees(lon) + 540.0) % 360.0 - 180.0, 6);

        return (latDeg, lonDeg);
    }

    /// <summary>
    /// Simulates realistic altitude in feet based on flight progress percentage (0-100%).
    /// </summary>
    public static int EstimateAltitudeFeet(double progressFraction, int cruisingAltitudeFeet = 36000)
    {
        if (progressFraction <= 0.0) return 0;
        if (progressFraction >= 1.0) return 0;

        // Climb: 0 -> 0.15
        if (progressFraction < 0.15)
        {
            double climbRatio = progressFraction / 0.15;
            return (int)Math.Round(cruisingAltitudeFeet * Math.Sin(climbRatio * Math.PI / 2.0));
        }

        // Cruise: 0.15 -> 0.85
        if (progressFraction <= 0.85)
        {
            return cruisingAltitudeFeet;
        }

        // Descent: 0.85 -> 1.0
        double descentRatio = (progressFraction - 0.85) / 0.15;
        return (int)Math.Round(cruisingAltitudeFeet * Math.Cos(descentRatio * Math.PI / 2.0));
    }

    /// <summary>
    /// Simulates realistic ground speed in knots based on flight progress percentage (0-100%).
    /// </summary>
    public static int EstimateSpeedKnots(double progressFraction, int cruisingSpeedKnots = 470)
    {
        if (progressFraction <= 0.0 || progressFraction >= 1.0) return 0;

        // Takeoff & initial climb: 160 -> cruisingSpeed
        if (progressFraction < 0.15)
        {
            double ratio = progressFraction / 0.15;
            return (int)Math.Round(160 + (cruisingSpeedKnots - 160) * ratio);
        }

        // Cruise
        if (progressFraction <= 0.85)
        {
            return cruisingSpeedKnots;
        }

        // Descent & approach: cruisingSpeed -> 150
        double descentRatio = (progressFraction - 0.85) / 0.15;
        return (int)Math.Round(cruisingSpeedKnots - (cruisingSpeedKnots - 150) * descentRatio);
    }
}
