using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Partner.Routes;

[Collection("FlightsTests")]
public class GetPartnerRoutesTests : BaseIntegrationTest
{
    public GetPartnerRoutesTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetPartnerRoutes_ShouldReturnOk_WhenPartnerWithAirline()
    {
        // Arrange
        var airlineId = Guid.NewGuid();

        // Act
        var response = await Client.AsPartner(airlineId).GetAsync("/api/partner/routes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/partner/routes: GetPartnerRoutes_ShouldReturnOk_WhenPartnerWithAirline must return 200 OK but return {0}", response.StatusCode);
    }
}
