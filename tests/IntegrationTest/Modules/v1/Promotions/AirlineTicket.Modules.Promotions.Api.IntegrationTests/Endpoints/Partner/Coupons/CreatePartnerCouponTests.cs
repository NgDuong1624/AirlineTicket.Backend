using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Promotions.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Promotions.Api.IntegrationTests.Endpoints.Partner.Coupons;

[Collection("PromotionsTests")]
public class CreatePartnerCouponTests : BaseIntegrationTest
{
    public CreatePartnerCouponTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreatePartnerCoupon_ShouldSucceed_WhenPartner()
    {
        // Arrange
        var airlineId = Guid.NewGuid();
        var request = new PartnerCouponRequest(
            Code: $"AIR_{Guid.NewGuid():N}"[..10].ToUpperInvariant(),
            Description: "Partner Coupon",
            DiscountType: 1,
            DiscountValue: 10m,
            MinOrderValue: 500000m,
            MaxDiscountAmount: 100000m,
            StartDate: DateTime.UtcNow.AddDays(-1),
            EndDate: DateTime.UtcNow.AddDays(30),
            UsageLimit: 100,
            IsActive: true);

        // Act
        var response = await Client.AsPartner(airlineId).PostAsJsonAsync("/api/partner/coupons", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created, "POST /api/partner/coupons: CreatePartnerCoupon must return 201 Created but return {0}", response.StatusCode);
    }
}
