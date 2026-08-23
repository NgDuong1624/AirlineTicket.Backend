using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Partner.Aircraft.Id;

[Collection("FlightsTests")]
public class DeletePartnerAircraftTests : BaseIntegrationTest
{
    public DeletePartnerAircraftTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task DeletePartnerAircraft_ShouldReturnBadRequest_WhenAircraftDoesNotExist()
    {
        // Arrange
        var airlineId = Guid.NewGuid();

        // Act
        var response = await Client.AsPartner(airlineId).DeleteAsync($"/api/partner/aircraft/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "DELETE /api/partner/aircraft/{Guid.NewGuid()}: DeletePartnerAircraft_ShouldReturnBadRequest_WhenAircraftDoesNotExist must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
