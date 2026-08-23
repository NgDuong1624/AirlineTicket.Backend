using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Promotions.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Promotions.Api.IntegrationTests.Endpoints.Admin.Campaigns.Id;

[Collection("PromotionsTests")]
public class AdminUpdateCampaignTests : BaseIntegrationTest
{
    public AdminUpdateCampaignTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AdminUpdateCampaign_ShouldReturnBadRequest_WhenCampaignDoesNotExist()
    {
        // Arrange
        var request = new AdminCampaignRequest(
            Title: "Autumn Holiday Special Updated",
            BannerUrl: "https://autumn.png",
            Content: "Save up to 45%",
            StartDate: DateTime.UtcNow.AddDays(-1),
            EndDate: DateTime.UtcNow.AddDays(45),
            IsFeatured: true);

        // Act
        var response = await Client.AsAdmin().PutAsJsonAsync($"/api/admin/campaigns/{Guid.NewGuid()}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "PUT /api/admin/campaigns/{{id}}: AdminUpdateCampaign must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
