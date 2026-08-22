using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Flights.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests;

public class FlightEndpointsTests : BaseIntegrationTest
{
    public FlightEndpointsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetAirports_ShouldReturnOkWithList()
    {
        // Act
        var response = await Client.AsAnonymous().GetAsync("/api/airports");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        content.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Fact]
    public async Task GetAirlines_ShouldReturnOkWithAirlines()
    {
        // Act
        var response = await Client.AsAnonymous().GetAsync("/api/airlines");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        content.TryGetProperty("items", out var itemsProp).Should().BeTrue();
        itemsProp.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task GetFlightById_ShouldReturnNotFound_WhenFlightDoesNotExist()
    {
        // Act
        var response = await Client.AsAnonymous().GetAsync($"/api/flights/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SearchFlights_ShouldReturnOk_WhenRequestIsValid()
    {
        // Arrange
        var request = new FlightSearchRequest(
            OriginCode: "HAN",
            DestinationCode: "SGN",
            DepartDate: DateTime.UtcNow.AddDays(7));

        // Act
        var response = await Client.AsAnonymous().PostAsJsonAsync("/api/flights", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
