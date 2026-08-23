using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Promotions.Api.IntegrationTests.Endpoints.Partner.Coupons.Id;

[Collection("PromotionsTests")]
public class DeletePartnerCouponTests : BaseIntegrationTest
{
    public DeletePartnerCouponTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task DeletePartnerCoupon_ShouldReturnBadRequest_WhenCouponDoesNotExist()
    {
        // Arrange
        var airlineId = Guid.NewGuid();

        // Act
        var response = await Client.AsPartner(airlineId).DeleteAsync($"/api/partner/coupons/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "DELETE /api/partner/coupons/{{id}}: DeletePartnerCoupon must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
