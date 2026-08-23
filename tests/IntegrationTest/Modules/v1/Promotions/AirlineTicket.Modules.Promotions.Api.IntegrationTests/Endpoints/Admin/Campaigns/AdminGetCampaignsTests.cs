using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Promotions.Api.IntegrationTests.Endpoints.Admin.Campaigns;

[Collection("PromotionsTests")]
public class AdminGetCampaignsTests : BaseIntegrationTest
{
    public AdminGetCampaignsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AdminGetCampaigns_ShouldReturnOk_WhenAdmin()
    {
        // Act
        var response = await Client.AsAdmin().GetAsync("/api/admin/campaigns");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/admin/campaigns: AdminGetCampaigns must return 200 OK but return {0}", response.StatusCode);
    }
}
