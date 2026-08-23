using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Interactions.Api.IntegrationTests.Endpoints.Interactions;

[Collection("InteractionsTests")]
public class GetInteractionsStatusTests : BaseIntegrationTest
{
    public GetInteractionsStatusTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetStatus_ShouldReturnOk_WhenAnonymous()
    {
        // Act
        var response = await Client.AsAnonymous().GetAsync("/api/v1/interactions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/v1/interactions: GetInteractionsStatus must return 200 OK but return {0}", response.StatusCode);
    }
}
