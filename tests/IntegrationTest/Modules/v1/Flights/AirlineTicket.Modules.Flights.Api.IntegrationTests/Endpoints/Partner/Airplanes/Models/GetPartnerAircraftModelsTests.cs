using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Partner.Airplanes.Models;

[Collection("FlightsTests")]
public class GetPartnerAircraftModelsTests : BaseIntegrationTest
{
    public GetPartnerAircraftModelsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetAircraftModels_ShouldReturnOk_WhenPartnerWithAirline()
    {
        // Arrange
        var airlineId = Guid.NewGuid();

        // Act
        var response = await Client.AsPartner(airlineId).GetAsync("/api/partner/airplanes/models");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/partner/airplanes/models: GetAircraftModels_ShouldReturnOk_WhenPartnerWithAirline must return 200 OK but return {0}", response.StatusCode);
    }
}
