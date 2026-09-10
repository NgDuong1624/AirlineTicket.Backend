using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Flights.Api.Endpoints;
using FluentAssertions;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Radar;

[Collection("FlightsTests")]
public class FlightRadarEndpointTests : BaseIntegrationTest
{
    public FlightRadarEndpointTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetActiveAirborneFlights_ShouldReturnOk()
    {
        // Act
        var response = await Client.AsAnonymous().GetAsync("/api/flights/radar/active");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetFlightTelemetry_ShouldReturnNotFound_WhenFlightDoesNotExist()
    {
        // Act
        var response = await Client.AsAnonymous().GetAsync($"/api/flights/{Guid.NewGuid()}/telemetry");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetFlightStatusByNumber_ShouldReturnNotFound_WhenFlightDoesNotExist()
    {
        // Act
        var response = await Client.AsAnonymous().GetAsync("/api/flights/status/NONEXISTENT999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateFlightStatusAndGate_ShouldReturnUnauthorized_WhenAnonymous()
    {
        // Act
        var response = await Client.AsAnonymous().PatchAsJsonAsync(
            $"/api/flights/{Guid.NewGuid()}/status-gate",
            new UpdateFlightStatusAndGateRequest());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateFlightStatusAndGate_ShouldReturnNotFound_WhenFlightDoesNotExist()
    {
        // Act
        var response = await Client.AsAdmin().PatchAsJsonAsync(
            $"/api/flights/{Guid.NewGuid()}/status-gate",
            new UpdateFlightStatusAndGateRequest(DepartureGate: "Gate A1"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
