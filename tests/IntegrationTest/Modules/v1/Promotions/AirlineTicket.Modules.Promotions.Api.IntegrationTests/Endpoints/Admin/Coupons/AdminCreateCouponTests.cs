using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Promotions.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Promotions.Api.IntegrationTests.Endpoints.Admin.Coupons;

[Collection("PromotionsTests")]
public class AdminCreateCouponTests : BaseIntegrationTest
{
    public AdminCreateCouponTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AdminCreateCoupon_ShouldSucceed_WhenAdmin()
    {
        // Arrange
        var promoCode = $"PROMO_{Guid.NewGuid():N}"[..12].ToUpperInvariant();
        var request = new CreatePromotionRequest(
            Name: "Admin Test Coupon",
            PromoCode: promoCode,
            DiscountType: "Percentage",
            DiscountValue: 20,
            MaxUsage: 50,
            StartDate: DateTime.UtcNow.AddDays(-1),
            EndDate: DateTime.UtcNow.AddDays(30));

        // Act
        var response = await Client.AsAdmin().PostAsJsonAsync("/api/admin/coupons", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created, "POST /api/admin/coupons: AdminCreateCoupon must return 201 Created but return {0}", response.StatusCode);
    }
}
