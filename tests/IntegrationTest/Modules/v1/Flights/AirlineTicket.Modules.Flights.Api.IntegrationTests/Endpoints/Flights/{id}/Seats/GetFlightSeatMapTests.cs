using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Flights.Id.Seats;

[Collection("FlightsTests")]
public class GetFlightSeatMapTests : BaseIntegrationTest
{
    public GetFlightSeatMapTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetFlightSeats_ShouldReturnNotFound_WhenFlightDoesNotExist()
    {
        // Act
        var response = await Client.AsAnonymous().GetAsync($"/api/flights/{Guid.NewGuid()}/seats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "GET /api/flights/{Guid.NewGuid()}/seats: GetFlightSeats_ShouldReturnNotFound_WhenFlightDoesNotExist must return 404 NotFound but return {0}", response.StatusCode);
    }
}
