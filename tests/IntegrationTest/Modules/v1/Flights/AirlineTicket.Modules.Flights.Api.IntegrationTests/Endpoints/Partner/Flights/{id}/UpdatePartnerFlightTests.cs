using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Flights.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Partner.Flights.Id;

[Collection("FlightsTests")]
public class UpdatePartnerFlightTests : BaseIntegrationTest
{
    public UpdatePartnerFlightTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdatePartnerFlight_ShouldReturnBadRequest_WhenFlightDoesNotExist()
    {
        // Arrange
        var airlineId = Guid.NewGuid();
        var request = new PartnerFlightRequest(
            RouteId: Guid.NewGuid(),
            AirplaneId: Guid.NewGuid(),
            FlightNumber: "VN999",
            BasePrice: 1300000m,
            DepartureTime: DateTime.UtcNow.AddDays(5));

        // Act
        var response = await Client.AsPartner(airlineId).PutAsJsonAsync($"/api/partner/flights/{Guid.NewGuid()}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "PUT /api/partner/flights/{Guid.NewGuid()}: UpdatePartnerFlight_ShouldReturnBadRequest_WhenFlightDoesNotExist must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
