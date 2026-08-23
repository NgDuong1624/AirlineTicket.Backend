using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Users.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Users.Api.IntegrationTests.Endpoints.Auth.Logout;

[Collection("UsersTests")]
public class LogoutTests : BaseIntegrationTest
{
    public LogoutTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Logout_ShouldReturnOk_WhenRefreshTokenProvided()
    {
        // Arrange
        var request = new LogoutRequest(RefreshToken: "dummy_refresh_token_to_revoke");

        // Act
        var response = await Client.AsAnonymous().PostAsJsonAsync("/api/auth/logout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "POST /api/auth/logout: Logout_ShouldReturnOk_WhenRefreshTokenProvided must return 200 OK but return {0}", response.StatusCode);
    }
}
