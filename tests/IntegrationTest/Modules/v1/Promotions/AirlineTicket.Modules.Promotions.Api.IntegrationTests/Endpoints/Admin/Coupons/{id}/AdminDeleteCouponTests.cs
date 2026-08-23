using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Promotions.Api.IntegrationTests.Endpoints.Admin.Coupons.Id;

[Collection("PromotionsTests")]
public class AdminDeleteCouponTests : BaseIntegrationTest
{
    public AdminDeleteCouponTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AdminDeleteCoupon_ShouldReturnBadRequest_WhenCouponDoesNotExist()
    {
        // Act
        var response = await Client.AsAdmin().DeleteAsync($"/api/admin/coupons/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "DELETE /api/admin/coupons/{{id}}: AdminDeleteCoupon must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
