using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Promotions.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Promotions.Api.IntegrationTests.Endpoints.Promotions.Apply;

[Collection("PromotionsTests")]
public class ApplyPromotionTests : BaseIntegrationTest
{
    public ApplyPromotionTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ApplyPromotion_ShouldReturnBadRequest_WhenInvalidCode()
    {
        // Arrange
        var request = new ApplyPromotionRequest("INVALID_CODE", Guid.NewGuid(), 1000000m);

        // Act
        var response = await Client.AsAnonymous().PostAsJsonAsync("/api/promotions/apply", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "POST /api/promotions/apply: ApplyPromotion must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
