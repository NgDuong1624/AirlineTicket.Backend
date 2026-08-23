using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Users.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Users.Api.IntegrationTests.Endpoints.Auth.Login;

[Collection("UsersTests")]
public class LoginTests : BaseIntegrationTest
{
    public LoginTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Login_ShouldReturnTokens_WhenCredentialsAreValid()
    {
        // Arrange
        var email = $"login_{Guid.NewGuid():N}@test.com";
        var password = "Password123!";
        var registerRequest = new RegisterUserRequest(
            Email: email,
            Password: password,
            FullName: "Login Test User",
            Phone: "0912345678",
            LanguagePreference: "vi");

        await Client.AsAnonymous().PostAsJsonAsync("/api/auth/register", registerRequest);
        var loginRequest = new LoginUserRequest(Email: email, Password: password);

        // Act
        var response = await Client.AsAnonymous().PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "POST /api/auth/login: Login_ShouldReturnTokens_WhenCredentialsAreValid must return 200 OK but return {0}", response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        content.TryGetProperty("accessToken", out var tokenProp).Should().BeTrue();
        tokenProp.GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_ShouldFail_WhenPasswordIsIncorrect()
    {
        // Arrange
        var email = $"wrongpass_{Guid.NewGuid():N}@test.com";
        var registerRequest = new RegisterUserRequest(
            Email: email,
            Password: "Password123!",
            FullName: "Wrong Pass User",
            Phone: "0912345679",
            LanguagePreference: "en");

        await Client.AsAnonymous().PostAsJsonAsync("/api/auth/register", registerRequest);
        var loginRequest = new LoginUserRequest(Email: email, Password: "WrongPassword!");

        // Act
        var response = await Client.AsAnonymous().PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "POST /api/auth/login: Login_ShouldFail_WhenPasswordIsIncorrect must return 401 Unauthorized but return {0}", response.StatusCode);
    }
}
