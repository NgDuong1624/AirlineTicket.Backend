using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Promotions.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Promotions.Api.IntegrationTests.Endpoints.Partner.Campaigns.Id;

[Collection("PromotionsTests")]
public class UpdatePartnerCampaignTests : BaseIntegrationTest
{
    public UpdatePartnerCampaignTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdatePartnerCampaign_ShouldReturnBadRequest_WhenCampaignDoesNotExist()
    {
        // Arrange
        var airlineId = Guid.NewGuid();
        var request = new PartnerCampaignRequest(
            Title: "Summer Mega Sale Updated",
            BannerUrl: "https://banner.png",
            Content: "Up to 60% discount",
            StartDate: DateTime.UtcNow.AddDays(-1),
            EndDate: DateTime.UtcNow.AddDays(30),
            IsFeatured: true);

        // Act
        var response = await Client.AsPartner(airlineId).PutAsJsonAsync($"/api/partner/campaigns/{Guid.NewGuid()}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "PUT /api/partner/campaigns/{{id}}: UpdatePartnerCampaign must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
