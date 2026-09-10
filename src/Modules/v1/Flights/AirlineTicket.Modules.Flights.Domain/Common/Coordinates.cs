namespace AirlineTicket.Modules.Flights.Domain.Common;

public readonly record struct Coordinates
{
    public double Latitude { get; }
    public double Longitude { get; }

    public Coordinates(double latitude, double longitude)
    {
        if (latitude < -90.0 || latitude > 90.0)
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90 degrees.");

        if (longitude < -180.0 || longitude > 180.0)
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180 degrees.");

        Latitude = Math.Round(latitude, 6);
        Longitude = Math.Round(longitude, 6);
    }

    public double DistanceToKm(Coordinates target)
    {
        return GeoMath.HaversineDistanceKm(Latitude, Longitude, target.Latitude, target.Longitude);
    }

    public int BearingTo(Coordinates target)
    {
        return GeoMath.CalculateBearing(Latitude, Longitude, target.Latitude, target.Longitude);
    }

    public Coordinates InterpolateTowards(Coordinates target, double fraction)
    {
        var (lat, lon) = GeoMath.InterpolatePosition(Latitude, Longitude, target.Latitude, target.Longitude, fraction);
        return new Coordinates(lat, lon);
    }
}
