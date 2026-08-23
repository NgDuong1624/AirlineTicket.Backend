using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Promotions.Api.IntegrationTests.Endpoints.Partner.Campaigns;

[Collection("PromotionsTests")]
public class GetPartnerCampaignsTests : BaseIntegrationTest
{
    public GetPartnerCampaignsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetPartnerCampaigns_ShouldReturnOk_WhenPartner()
    {
        // Arrange
        var airlineId = Guid.NewGuid();

        // Act
        var response = await Client.AsPartner(airlineId).GetAsync("/api/partner/campaigns");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/partner/campaigns: GetPartnerCampaigns must return 200 OK but return {0}", response.StatusCode);
    }
}
