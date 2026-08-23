using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Partner.Airplanes;

[Collection("FlightsTests")]
public class GetPartnerAirplanesTests : BaseIntegrationTest
{
    public GetPartnerAirplanesTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetPartnerAirplanes_ShouldReturnOk_WhenPartnerWithAirline()
    {
        // Arrange
        var airlineId = Guid.NewGuid();

        // Act
        var response = await Client.AsPartner(airlineId).GetAsync("/api/partner/airplanes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/partner/airplanes: GetPartnerAirplanes_ShouldReturnOk_WhenPartnerWithAirline must return 200 OK but return {0}", response.StatusCode);
    }
}
