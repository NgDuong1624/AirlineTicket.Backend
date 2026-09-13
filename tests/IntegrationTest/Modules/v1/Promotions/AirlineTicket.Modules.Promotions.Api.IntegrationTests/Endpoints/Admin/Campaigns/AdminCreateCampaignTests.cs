using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Promotions.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Promotions.Api.IntegrationTests.Endpoints.Admin.Campaigns;

[Collection("PromotionsTests")]
public class AdminCreateCampaignTests : BaseIntegrationTest
{
    public AdminCreateCampaignTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AdminCreateCampaign_ShouldSucceed_WhenAdmin()
    {
        // Arrange
        var request = new AdminCampaignRequest(
            TitleEn: "Autumn Holiday Special",
            TitleVi: "Kỳ nghỉ mùa thu đặc biệt",
            BannerUrl: "https://autumn.png",
            ContentEn: "Save up to 40%",
            ContentVi: "Tiết kiệm tới 40%",
            StartDate: DateTime.UtcNow.AddDays(-1),
            EndDate: DateTime.UtcNow.AddDays(30),
            IsFeatured: true);

        // Act
        var response = await Client.AsAdmin().PostAsJsonAsync("/api/admin/campaigns", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created, "POST /api/admin/campaigns: AdminCreateCampaign must return 201 Created but return {0}", response.StatusCode);
    }
}
