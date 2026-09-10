using AirlineTicket.Modules.Flights.Domain.Common;
using FluentAssertions;

namespace AirlineTicket.Modules.Flights.Application.UnitTests.Radar;

public class GeoMathTests
{
    // Hanoi (HAN) coordinates
    private const double HanLat = 21.0285;
    private const double HanLon = 105.8542;

    // Ho Chi Minh City (SGN) coordinates
    private const double SgnLat = 10.8231;
    private const double SgnLon = 106.6297;

    [Fact]
    public void HaversineDistanceKm_ShouldReturnZero_ForIdenticalPoints()
    {
        var distance = GeoMath.HaversineDistanceKm(HanLat, HanLon, HanLat, HanLon);
        distance.Should().BeApproximately(0.0, 0.001);
    }

    [Fact]
    public void HaversineDistanceKm_ShouldCalculateAccurateDistance_BetweenHanAndSgn()
    {
        // Great-circle distance between Hanoi and Ho Chi Minh City is ~1130-1160 km
        var distance = GeoMath.HaversineDistanceKm(HanLat, HanLon, SgnLat, SgnLon);
        distance.Should().BeInRange(1130.0, 1160.0);
    }

    [Fact]
    public void CalculateBearing_ShouldReturnBearingWithinValidDegreesRange()
    {
        var bearing = GeoMath.CalculateBearing(HanLat, HanLon, SgnLat, SgnLon);
        bearing.Should().BeInRange(0, 359);
    }

    [Fact]
    public void CalculateBearing_ShouldPointSouth_WhenMovingDirectlySouth()
    {
        var bearing = GeoMath.CalculateBearing(20.0, 100.0, 10.0, 100.0);
        bearing.Should().BeInRange(175, 185);
    }

    [Fact]
    public void InterpolatePosition_ShouldReturnOrigin_AtZeroFraction()
    {
        var (lat, lon) = GeoMath.InterpolatePosition(HanLat, HanLon, SgnLat, SgnLon, 0.0);
        lat.Should().BeApproximately(HanLat, 0.001);
        lon.Should().BeApproximately(HanLon, 0.001);
    }

    [Fact]
    public void InterpolatePosition_ShouldReturnDestination_AtOneFraction()
    {
        var (lat, lon) = GeoMath.InterpolatePosition(HanLat, HanLon, SgnLat, SgnLon, 1.0);
        lat.Should().BeApproximately(SgnLat, 0.001);
        lon.Should().BeApproximately(SgnLon, 0.001);
    }

    [Fact]
    public void InterpolatePosition_ShouldReturnMidpoint_AtHalfFraction()
    {
        var (lat, lon) = GeoMath.InterpolatePosition(HanLat, HanLon, SgnLat, SgnLon, 0.5);
        lat.Should().BeInRange(SgnLat, HanLat);
        lon.Should().BeInRange(HanLon, SgnLon);

        var distOrigin = GeoMath.HaversineDistanceKm(HanLat, HanLon, lat, lon);
        var distDest = GeoMath.HaversineDistanceKm(lat, lon, SgnLat, SgnLon);
        distOrigin.Should().BeApproximately(distDest, 5.0);
    }

    [Theory]
    [InlineData(0.0, 0)]
    [InlineData(1.0, 0)]
    [InlineData(0.5, 36000)]
    public void EstimateAltitudeFeet_ShouldEstimateAppropriateAltitude(double fraction, int expectedAltitude)
    {
        var altitude = GeoMath.EstimateAltitudeFeet(fraction, 36000);
        if (expectedAltitude == 0)
        {
            altitude.Should().Be(0);
        }
        else
        {
            altitude.Should().BeInRange(30000, 36000);
        }
    }

    [Theory]
    [InlineData(0.0, 0)]
    [InlineData(1.0, 0)]
    [InlineData(0.5, 470)]
    public void EstimateSpeedKnots_ShouldEstimateAppropriateSpeed(double fraction, int expectedSpeed)
    {
        var speed = GeoMath.EstimateSpeedKnots(fraction, 470);
        if (expectedSpeed == 0)
        {
            speed.Should().Be(0);
        }
        else
        {
            speed.Should().BeInRange(400, 480);
        }
    }

    [Fact]
    public void Coordinates_ShouldThrowArgumentOutOfRangeException_WhenInvalidLatitude()
    {
        var act = () => new Coordinates(95.0, 100.0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Coordinates_ShouldThrowArgumentOutOfRangeException_WhenInvalidLongitude()
    {
        var act = () => new Coordinates(10.0, -190.0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Coordinates_ShouldCalculateDistanceAndBearing()
    {
        var han = new Coordinates(HanLat, HanLon);
        var sgn = new Coordinates(SgnLat, SgnLon);

        var dist = han.DistanceToKm(sgn);
        dist.Should().BeInRange(1130.0, 1160.0);

        var bearing = han.BearingTo(sgn);
        bearing.Should().BeInRange(0, 359);
    }
}
