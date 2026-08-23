using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Users.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Users.Api.IntegrationTests.Endpoints.Auth.Me;

[Collection("UsersTests")]
public class GetMeTests : BaseIntegrationTest
{
    public GetMeTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetMe_ShouldReturnUnauthorized_WhenAnonymous()
    {
        // Act
        var response = await Client.AsAnonymous().GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "GET /api/auth/me: GetMe_ShouldReturnUnauthorized_WhenAnonymous must return 401 Unauthorized but return {0}", response.StatusCode);
    }

    [Fact]
    public async Task GetMe_ShouldReturnUserProfile_WhenAuthenticated()
    {
        // Arrange
        var email = $"me_{Guid.NewGuid():N}@test.com";
        var registerRequest = new RegisterUserRequest(
            Email: email,
            Password: "Password123!",
            FullName: "Me Test User",
            Phone: "0933333333",
            LanguagePreference: "en");

        var regResponse = await Client.AsAnonymous().PostAsJsonAsync("/api/auth/register", registerRequest);
        var regContent = await regResponse.Content.ReadFromJsonAsync<JsonElement>();
        var userId = regContent.GetProperty("userId").GetGuid();

        // Act
        var response = await Client.AsCustomer(userId).GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/auth/me: GetMe_ShouldReturnUserProfile_WhenAuthenticated must return 200 OK but return {0}", response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<JsonElement>();
        profile.GetProperty("email").GetString().Should().Be(email);
    }
}
