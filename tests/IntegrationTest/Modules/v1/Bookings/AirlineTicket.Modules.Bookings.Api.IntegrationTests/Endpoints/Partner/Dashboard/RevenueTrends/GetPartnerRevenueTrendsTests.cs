using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Api.IntegrationTests.Endpoints.Partner.Dashboard.RevenueTrends;

[Collection("BookingsTests")]
public class GetPartnerRevenueTrendsTests : BaseIntegrationTest
{
    public GetPartnerRevenueTrendsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetRevenueTrends_ShouldReturnOk_WhenPartnerWithAirline()
    {
        // Arrange
        var airlineId = Guid.NewGuid();

        // Act
        var response = await Client.AsPartner(airlineId).GetAsync("/api/partner/dashboard/revenue-trends");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/partner/dashboard/revenue-trends: GetRevenueTrends_ShouldReturnOk_WhenPartnerWithAirline must return 200 OK but return {0}", response.StatusCode);
    }
}
