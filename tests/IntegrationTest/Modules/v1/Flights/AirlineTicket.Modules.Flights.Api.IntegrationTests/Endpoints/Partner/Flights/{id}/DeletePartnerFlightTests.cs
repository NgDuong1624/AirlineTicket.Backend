using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Partner.Flights.Id;

[Collection("FlightsTests")]
public class DeletePartnerFlightTests : BaseIntegrationTest
{
    public DeletePartnerFlightTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task DeletePartnerFlight_ShouldReturnBadRequest_WhenFlightDoesNotExist()
    {
        // Arrange
        var airlineId = Guid.NewGuid();

        // Act
        var response = await Client.AsPartner(airlineId).DeleteAsync($"/api/partner/flights/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "DELETE /api/partner/flights/{Guid.NewGuid()}: DeletePartnerFlight_ShouldReturnBadRequest_WhenFlightDoesNotExist must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
