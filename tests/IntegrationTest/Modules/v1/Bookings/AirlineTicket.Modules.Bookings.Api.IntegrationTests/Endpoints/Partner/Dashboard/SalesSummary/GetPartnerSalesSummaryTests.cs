using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Api.IntegrationTests.Endpoints.Partner.Dashboard.SalesSummary;

[Collection("BookingsTests")]
public class GetPartnerSalesSummaryTests : BaseIntegrationTest
{
    public GetPartnerSalesSummaryTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetSalesSummary_ShouldReturnForbidden_WhenCustomer()
    {
        // Act
        var response = await Client.AsCustomer().GetAsync("/api/partner/dashboard/sales-summary");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden, "GET /api/partner/dashboard/sales-summary: GetSalesSummary_ShouldReturnForbidden_WhenCustomer must return 403 Forbidden but return {0}", response.StatusCode);
    }

    [Fact]
    public async Task GetSalesSummary_ShouldReturnOk_WhenPartnerWithAirline()
    {
        // Arrange
        var airlineId = Guid.NewGuid();

        // Act
        var response = await Client.AsPartner(airlineId).GetAsync("/api/partner/dashboard/sales-summary");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/partner/dashboard/sales-summary: GetSalesSummary_ShouldReturnOk_WhenPartnerWithAirline must return 200 OK but return {0}", response.StatusCode);
    }
}
