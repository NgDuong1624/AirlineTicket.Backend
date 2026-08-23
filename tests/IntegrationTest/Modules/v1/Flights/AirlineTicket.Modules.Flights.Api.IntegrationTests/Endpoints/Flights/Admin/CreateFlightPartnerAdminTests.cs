using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Flights.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Flights.Admin;

[Collection("FlightsTests")]
public class CreateFlightPartnerAdminTests : BaseIntegrationTest
{
    public CreateFlightPartnerAdminTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateFlight_ShouldReturnForbidden_WhenAnonymousOrCustomer()
    {
        // Arrange
        var request = new CreateFlightRequest(
            RouteId: Guid.NewGuid(),
            AirplaneId: Guid.NewGuid(),
            FlightNumber: "VN123",
            BasePrice: 1500000m,
            ScheduledDeparture: DateTime.UtcNow.AddDays(10),
            ScheduledArrival: DateTime.UtcNow.AddDays(10).AddHours(2));

        // Act
        var response = await Client.AsCustomer().PostAsJsonAsync("/api/flights/admin", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden, "POST /api/flights/admin: CreateFlight_ShouldReturnForbidden_WhenAnonymousOrCustomer must return 403 Forbidden but return {0}", response.StatusCode);
    }
}
