using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Promotions.Api.IntegrationTests.Endpoints.Promotions.Campaigns;

[Collection("PromotionsTests")]
public class GetCampaignsTests : BaseIntegrationTest
{
    public GetCampaignsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetCampaigns_ShouldReturnOk_WhenAnonymous()
    {
        // Act
        var response = await Client.AsAnonymous().GetAsync("/api/promotions/campaigns");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/promotions/campaigns: GetCampaigns must return 200 OK but return {0}", response.StatusCode);
    }
}
