using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Promotions.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Promotions.Api.IntegrationTests.Endpoints.Admin.Coupons.Id;

[Collection("PromotionsTests")]
public class AdminUpdateCouponTests : BaseIntegrationTest
{
    public AdminUpdateCouponTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AdminUpdateCoupon_ShouldReturnBadRequest_WhenCouponDoesNotExist()
    {
        // Arrange
        var request = new UpdatePromotionRequest(
            Name: "Updated Admin Coupon",
            DiscountValue: 25,
            EndDate: DateTime.UtcNow.AddDays(45));

        // Act
        var response = await Client.AsAdmin().PutAsJsonAsync($"/api/admin/coupons/{Guid.NewGuid()}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "PUT /api/admin/coupons/{{id}}: AdminUpdateCoupon must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
