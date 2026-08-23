using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Flights.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Partner.Flights;

[Collection("FlightsTests")]
public class CreatePartnerFlightTests : BaseIntegrationTest
{
    public CreatePartnerFlightTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreatePartnerFlight_ShouldReturnBadRequest_WhenRouteDoesNotExist()
    {
        // Arrange
        var airlineId = Guid.NewGuid();
        var request = new PartnerFlightRequest(
            RouteId: Guid.NewGuid(),
            AirplaneId: Guid.NewGuid(),
            FlightNumber: "VN999",
            BasePrice: 1200000m,
            DepartureTime: DateTime.UtcNow.AddDays(5));

        // Act
        var response = await Client.AsPartner(airlineId).PostAsJsonAsync("/api/partner/flights", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "POST /api/partner/flights: CreatePartnerFlight_ShouldReturnBadRequest_WhenRouteDoesNotExist must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
