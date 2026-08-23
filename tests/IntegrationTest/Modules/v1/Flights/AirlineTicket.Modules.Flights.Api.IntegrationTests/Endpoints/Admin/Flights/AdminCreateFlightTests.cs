using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Flights.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Admin.Flights;

[Collection("FlightsTests")]
public class AdminCreateFlightTests : BaseIntegrationTest
{
    public AdminCreateFlightTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AdminCreateFlight_ShouldReturnBadRequest_WhenRouteDoesNotExist()
    {
        // Arrange
        var request = new CreateFlightRequest(
            RouteId: Guid.NewGuid(),
            AirplaneId: Guid.NewGuid(),
            FlightNumber: "VN777",
            BasePrice: 1500000m,
            ScheduledDeparture: DateTime.UtcNow.AddDays(7),
            ScheduledArrival: DateTime.UtcNow.AddDays(7).AddHours(2));

        // Act
        var response = await Client.AsAdmin().PostAsJsonAsync("/api/admin/flights", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "POST /api/admin/flights: AdminCreateFlight_ShouldReturnBadRequest_WhenRouteDoesNotExist must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
