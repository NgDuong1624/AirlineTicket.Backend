using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Api.IntegrationTests.Endpoints.Partner.Dashboard.OccupancyRates;

[Collection("BookingsTests")]
public class GetPartnerOccupancyRatesTests : BaseIntegrationTest
{
    public GetPartnerOccupancyRatesTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetOccupancyRates_ShouldReturnOk_WhenPartnerWithAirline()
    {
        // Arrange
        var airlineId = Guid.NewGuid();

        // Act
        var response = await Client.AsPartner(airlineId).GetAsync("/api/partner/dashboard/occupancy-rates");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/partner/dashboard/occupancy-rates: GetOccupancyRates_ShouldReturnOk_WhenPartnerWithAirline must return 200 OK but return {0}", response.StatusCode);
    }
}
