using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Promotions.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Promotions.Api.IntegrationTests;

public class PromotionEndpointsTests : BaseIntegrationTest
{
    public PromotionEndpointsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetActivePromotions_ShouldReturnOk()
    {
        // Act
        var response = await Client.AsAnonymous().GetAsync("/api/promotions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCampaigns_ShouldReturnOk()
    {
        // Act
        var response = await Client.AsAnonymous().GetAsync("/api/promotions/campaigns");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AdminGetCoupons_ShouldReturnForbidden_WhenUserIsNotAdmin()
    {
        // Act
        var response = await Client.AsCustomer().GetAsync("/api/admin/coupons");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminCreateCoupon_And_GetCoupons_ShouldSucceed_WhenUserIsAdmin()
    {
        // Arrange
        var promoCode = $"PROMO_{Guid.NewGuid():N}"[..12].ToUpperInvariant();
        var request = new CreatePromotionRequest(
            Name: "Integration Test Coupon",
            PromoCode: promoCode,
            DiscountType: "Percentage",
            DiscountValue: 15,
            MaxUsage: 100,
            StartDate: DateTime.UtcNow.AddDays(-1),
            EndDate: DateTime.UtcNow.AddDays(30));

        // Act
        var createResponse = await Client.AsAdmin().PostAsJsonAsync("/api/admin/coupons", request);

        // Assert
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var listResponse = await Client.AsAdmin().GetAsync("/api/admin/coupons");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
