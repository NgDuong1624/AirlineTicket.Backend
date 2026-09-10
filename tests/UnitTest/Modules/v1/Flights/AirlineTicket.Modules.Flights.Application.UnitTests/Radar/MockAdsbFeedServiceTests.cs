using AirlineTicket.Modules.Flights.Infrastructure.Services;
using FluentAssertions;

namespace AirlineTicket.Modules.Flights.Application.UnitTests.Radar;

public class MockAdsbFeedServiceTests
{
    private readonly MockAdsbFeedService _service = new();

    private const double HanLat = 21.0285;
    private const double HanLon = 105.8542;
    private const double SgnLat = 10.8231;
    private const double SgnLon = 106.6297;

    [Fact]
    public void ComputeFlightPosition_ShouldReturnOnGround_WhenCurrentTimeIsBeforeDeparture()
    {
        var flightId = Guid.NewGuid();
        var depTime = DateTime.UtcNow.AddHours(1);
        var arrTime = depTime.AddHours(2);
        var now = DateTime.UtcNow;

        var state = _service.ComputeFlightPosition(
            flightId, "VN123", "VN",
            HanLat, HanLon, SgnLat, SgnLon,
            depTime, arrTime, now);

        state.OnGround.Should().BeTrue();
        state.AltitudeFeet.Should().Be(0);
        state.VelocityKnots.Should().Be(0);
        state.ProgressPercentage.Should().Be(0);
        state.Latitude.Should().BeApproximately(HanLat, 0.001);
        state.Longitude.Should().BeApproximately(HanLon, 0.001);
        state.Callsign.Should().Be("VN123");
        state.Icao24.Should().HaveLength(6);
        state.Squawk.Should().Be("1200");
    }

    [Fact]
    public void ComputeFlightPosition_ShouldReturnOnGround_WhenCurrentTimeIsAfterArrival()
    {
        var flightId = Guid.NewGuid();
        var depTime = DateTime.UtcNow.AddHours(-3);
        var arrTime = depTime.AddHours(2);
        var now = DateTime.UtcNow;

        var state = _service.ComputeFlightPosition(
            flightId, "VN123", "VN",
            HanLat, HanLon, SgnLat, SgnLon,
            depTime, arrTime, now);

        state.OnGround.Should().BeTrue();
        state.AltitudeFeet.Should().Be(0);
        state.VelocityKnots.Should().Be(0);
        state.ProgressPercentage.Should().Be(100);
        state.Latitude.Should().BeApproximately(SgnLat, 0.001);
        state.Longitude.Should().BeApproximately(SgnLon, 0.001);
    }

    [Fact]
    public void ComputeFlightPosition_ShouldReturnAirborneAndCruising_WhenAtMidpoint()
    {
        var flightId = Guid.NewGuid();
        var depTime = DateTime.UtcNow.AddHours(-1);
        var arrTime = depTime.AddHours(2);
        var now = depTime.AddHours(1); // exactly 50%

        var state = _service.ComputeFlightPosition(
            flightId, "QH202", "QH",
            HanLat, HanLon, SgnLat, SgnLon,
            depTime, arrTime, now);

        state.OnGround.Should().BeFalse();
        state.ProgressPercentage.Should().Be(50);
        state.AltitudeFeet.Should().BeGreaterThan(30000);
        state.VelocityKnots.Should().BeGreaterThan(400);
        state.VerticalRateFpm.Should().Be(0); // Level cruise
        state.Squawk.Should().HaveLength(4);
        state.TrueTrackDegrees.Should().BeInRange(0, 359);
    }

    [Fact]
    public void ComputeFlightPosition_ShouldHavePositiveVerticalRate_DuringClimbPhase()
    {
        var flightId = Guid.NewGuid();
        var depTime = DateTime.UtcNow;
        var arrTime = depTime.AddMinutes(120);
        var now = depTime.AddMinutes(10); // ~8% progress -> climbing

        var state = _service.ComputeFlightPosition(
            flightId, "VJ151", "VJ",
            HanLat, HanLon, SgnLat, SgnLon,
            depTime, arrTime, now);

        state.OnGround.Should().BeFalse();
        state.VerticalRateFpm.Should().BeGreaterThan(0);
        state.AltitudeFeet.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ComputeFlightPosition_ShouldHaveNegativeVerticalRate_DuringDescentPhase()
    {
        var flightId = Guid.NewGuid();
        var depTime = DateTime.UtcNow;
        var arrTime = depTime.AddMinutes(120);
        var now = depTime.AddMinutes(110); // ~91% progress -> descending

        var state = _service.ComputeFlightPosition(
            flightId, "VJ151", "VJ",
            HanLat, HanLon, SgnLat, SgnLon,
            depTime, arrTime, now);

        state.OnGround.Should().BeFalse();
        state.VerticalRateFpm.Should().BeLessThan(0);
    }

    [Fact]
    public void ComputeFlightPosition_ShouldProduceDeterministicIcao24_ForSameFlightNumber()
    {
        var flightId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var state1 = _service.ComputeFlightPosition(
            flightId, "VN220", "VN",
            HanLat, HanLon, SgnLat, SgnLon,
            now, now.AddHours(2), now.AddHours(1));

        var state2 = _service.ComputeFlightPosition(
            flightId, "VN220", "VN",
            HanLat, HanLon, SgnLat, SgnLon,
            now, now.AddHours(2), now.AddHours(1));

        state1.Icao24.Should().Be(state2.Icao24);
        state1.Squawk.Should().Be(state2.Squawk);
    }
}
