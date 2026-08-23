using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Users.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Users.Api.IntegrationTests.Endpoints.Auth.Refresh;

[Collection("UsersTests")]
public class RefreshTokenTests : BaseIntegrationTest
{
    public RefreshTokenTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Refresh_ShouldFail_WhenTokenIsInvalid()
    {
        // Arrange
        var request = new RefreshTokenRequest(RefreshToken: "invalid_refresh_token");

        // Act
        var response = await Client.AsAnonymous().PostAsJsonAsync("/api/auth/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "POST /api/auth/refresh: Refresh_ShouldFail_WhenTokenIsInvalid must return 401 Unauthorized but return {0}", response.StatusCode);
    }
}
