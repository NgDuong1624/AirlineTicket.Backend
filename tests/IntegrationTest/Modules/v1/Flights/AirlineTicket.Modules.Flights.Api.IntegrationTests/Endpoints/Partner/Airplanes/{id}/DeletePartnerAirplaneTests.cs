using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Partner.Airplanes.Id;

[Collection("FlightsTests")]
public class DeletePartnerAirplaneTests : BaseIntegrationTest
{
    public DeletePartnerAirplaneTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task DeletePartnerAirplane_ShouldReturnBadRequest_WhenAirplaneDoesNotExist()
    {
        // Arrange
        var airlineId = Guid.NewGuid();

        // Act
        var response = await Client.AsPartner(airlineId).DeleteAsync($"/api/partner/airplanes/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "DELETE /api/partner/airplanes/{Guid.NewGuid()}: DeletePartnerAirplane_ShouldReturnBadRequest_WhenAirplaneDoesNotExist must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
