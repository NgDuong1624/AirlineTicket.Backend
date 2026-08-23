using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Promotions.Api.IntegrationTests.Endpoints.Partner.Campaigns.Id;

[Collection("PromotionsTests")]
public class DeletePartnerCampaignTests : BaseIntegrationTest
{
    public DeletePartnerCampaignTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task DeletePartnerCampaign_ShouldReturnBadRequest_WhenCampaignDoesNotExist()
    {
        // Arrange
        var airlineId = Guid.NewGuid();

        // Act
        var response = await Client.AsPartner(airlineId).DeleteAsync($"/api/partner/campaigns/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "DELETE /api/partner/campaigns/{{id}}: DeletePartnerCampaign must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
