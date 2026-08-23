using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Promotions.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Promotions.Api.IntegrationTests.Endpoints.Partner.Coupons.Id;

[Collection("PromotionsTests")]
public class UpdatePartnerCouponTests : BaseIntegrationTest
{
    public UpdatePartnerCouponTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdatePartnerCoupon_ShouldReturnBadRequest_WhenCouponDoesNotExist()
    {
        // Arrange
        var airlineId = Guid.NewGuid();
        var request = new PartnerCouponRequest(
            Code: "AIR_UPDATE",
            Description: "Partner Coupon Updated",
            DiscountType: 1,
            DiscountValue: 15m,
            MinOrderValue: 500000m,
            MaxDiscountAmount: 150000m,
            StartDate: DateTime.UtcNow.AddDays(-1),
            EndDate: DateTime.UtcNow.AddDays(30),
            UsageLimit: 200,
            IsActive: true);

        // Act
        var response = await Client.AsPartner(airlineId).PutAsJsonAsync($"/api/partner/coupons/{Guid.NewGuid()}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "PUT /api/partner/coupons/{{id}}: UpdatePartnerCoupon must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
