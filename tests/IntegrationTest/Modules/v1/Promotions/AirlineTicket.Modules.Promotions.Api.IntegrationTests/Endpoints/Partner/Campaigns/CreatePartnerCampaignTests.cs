using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Promotions.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Promotions.Api.IntegrationTests.Endpoints.Partner.Campaigns;

[Collection("PromotionsTests")]
public class CreatePartnerCampaignTests : BaseIntegrationTest
{
    public CreatePartnerCampaignTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreatePartnerCampaign_ShouldSucceed_WhenPartner()
    {
        // Arrange
        var airlineId = Guid.NewGuid();
        var request = new PartnerCampaignRequest(
            TitleEn: "Summer Mega Sale",
            TitleVi: "Đại tiệc mùa hè",
            BannerUrl: "https://banner.png",
            ContentEn: "Up to 50% discount",
            ContentVi: "Giảm giá tới 50%",
            StartDate: DateTime.UtcNow.AddDays(-1),
            EndDate: DateTime.UtcNow.AddDays(30),
            IsFeatured: true);

        // Act
        var response = await Client.AsPartner(airlineId).PostAsJsonAsync("/api/partner/campaigns", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created, "POST /api/partner/campaigns: CreatePartnerCampaign must return 201 Created but return {0}", response.StatusCode);
    }
}
