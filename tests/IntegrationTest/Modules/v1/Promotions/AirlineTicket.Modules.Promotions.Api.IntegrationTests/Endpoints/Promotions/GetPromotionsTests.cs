using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Promotions.Api.IntegrationTests.Endpoints.Promotions;

[Collection("PromotionsTests")]
public class GetPromotionsTests : BaseIntegrationTest
{
    public GetPromotionsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetPromotions_ShouldReturnOk_WhenAnonymous()
    {
        // Act
        var response = await Client.AsAnonymous().GetAsync("/api/promotions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/promotions: GetActivePromotions must return 200 OK but return {0}", response.StatusCode);
    }
}
