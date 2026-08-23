using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Api.IntegrationTests.Endpoints.Staff.Sales;

[Collection("BookingsTests")]
public class GetStaffSalesBoardTests : BaseIntegrationTest
{
    public GetStaffSalesBoardTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetStaffSales_ShouldReturnForbidden_WhenCustomer()
    {
        // Act
        var response = await Client.AsCustomer().GetAsync("/api/staff/sales");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden, "GET /api/staff/sales: GetStaffSales_ShouldReturnForbidden_WhenCustomer must return 403 Forbidden but return {0}", response.StatusCode);
    }
}
