using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Partner.Aircraft;

[Collection("FlightsTests")]
public class GetPartnerAircraftTests : BaseIntegrationTest
{
    public GetPartnerAircraftTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetPartnerAircraft_ShouldReturnOk_WhenPartnerWithAirline()
    {
        // Arrange
        var airlineId = Guid.NewGuid();

        // Act
        var response = await Client.AsPartner(airlineId).GetAsync("/api/partner/aircraft");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/partner/aircraft: GetPartnerAircraft_ShouldReturnOk_WhenPartnerWithAirline must return 200 OK but return {0}", response.StatusCode);
    }
}
