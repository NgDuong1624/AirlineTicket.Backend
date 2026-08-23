using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Flights.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Partner.Aircraft.Id;

[Collection("FlightsTests")]
public class UpdatePartnerAircraftTests : BaseIntegrationTest
{
    public UpdatePartnerAircraftTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdatePartnerAircraft_ShouldReturnBadRequest_WhenAircraftDoesNotExist()
    {
        // Arrange
        var airlineId = Guid.NewGuid();

        // Act
        var response = await Client.AsPartner(airlineId).PutAsync($"/api/partner/aircraft/{Guid.NewGuid()}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "PUT /api/partner/aircraft/{Guid.NewGuid()}: UpdatePartnerAircraft_ShouldReturnBadRequest_WhenAircraftDoesNotExist must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
