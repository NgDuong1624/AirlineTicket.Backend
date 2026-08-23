using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Users.Api.IntegrationTests.Endpoints.Auth.Google;

[Collection("UsersTests")]
public class GoogleLoginTests : BaseIntegrationTest
{
    public GoogleLoginTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GoogleLogin_ShouldReturnUnauthorized_WhenInvalidTokenProvided()
    {
        // Arrange
        var request = new { IdToken = "invalid_google_token" };

        // Act
        var response = await Client.AsAnonymous().PostAsJsonAsync("/api/auth/google", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "POST /api/auth/google: GoogleLogin_ShouldReturnUnauthorized_WhenInvalidTokenProvided must return 401 Unauthorized but return {0}", response.StatusCode);
    }
}
