using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Promotions.Api.IntegrationTests.Endpoints.Admin.Coupons;

[Collection("PromotionsTests")]
public class AdminGetCouponsTests : BaseIntegrationTest
{
    public AdminGetCouponsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AdminGetCoupons_ShouldReturnOk_WhenAdmin()
    {
        // Act
        var response = await Client.AsAdmin().GetAsync("/api/admin/coupons");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/admin/coupons: AdminGetCoupons must return 200 OK but return {0}", response.StatusCode);
    }
}
