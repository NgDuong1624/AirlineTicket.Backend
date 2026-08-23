using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Promotions.Api.IntegrationTests.Endpoints.Partner.Coupons;

[Collection("PromotionsTests")]
public class GetPartnerCouponsTests : BaseIntegrationTest
{
    public GetPartnerCouponsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetPartnerCoupons_ShouldReturnOk_WhenPartner()
    {
        // Arrange
        var airlineId = Guid.NewGuid();

        // Act
        var response = await Client.AsPartner(airlineId).GetAsync("/api/partner/coupons");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/partner/coupons: GetPartnerCoupons must return 200 OK but return {0}", response.StatusCode);
    }
}
