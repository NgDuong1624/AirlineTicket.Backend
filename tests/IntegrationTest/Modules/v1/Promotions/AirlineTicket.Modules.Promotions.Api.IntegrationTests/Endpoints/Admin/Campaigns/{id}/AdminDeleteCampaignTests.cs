using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Promotions.Api.IntegrationTests.Endpoints.Admin.Campaigns.Id;

[Collection("PromotionsTests")]
public class AdminDeleteCampaignTests : BaseIntegrationTest
{
    public AdminDeleteCampaignTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AdminDeleteCampaign_ShouldReturnBadRequest_WhenCampaignDoesNotExist()
    {
        // Act
        var response = await Client.AsAdmin().DeleteAsync($"/api/admin/campaigns/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "DELETE /api/admin/campaigns/{{id}}: AdminDeleteCampaign must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
