using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AirlineTicket.BuildingBlocks.Domain.Constants;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Users.Api.Endpoints;
using AirlineTicket.Modules.Users.Application.Features.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Users.Api.IntegrationTests;

public class UserEndpointsTests : BaseIntegrationTest
{
    public UserEndpointsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Register_ShouldSucceed_WhenRequestIsValid()
    {
        // Arrange
        var email = $"newuser_{Guid.NewGuid():N}@test.com";
        var request = new RegisterUserRequest(
            Email: email,
            Password: "Password123!",
            FullName: "Integration Test User",
            Phone: "0987654321",
            LanguagePreference: "en");

        // Act
        var response = await Client.AsAnonymous().PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        content.TryGetProperty("userId", out var userIdProp).Should().BeTrue();
        userIdProp.GetGuid().Should().NotBeEmpty();
    }

    [Fact]
    public async Task Register_ShouldFail_WhenEmailAlreadyExists()
    {
        // Arrange
        var email = $"duplicate_{Guid.NewGuid():N}@test.com";
        var request = new RegisterUserRequest(
            Email: email,
            Password: "Password123!",
            FullName: "Duplicate User",
            Phone: "0987654321",
            LanguagePreference: "en");

        var firstResponse = await Client.AsAnonymous().PostAsJsonAsync("/api/auth/register", request);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act
        var secondResponse = await Client.AsAnonymous().PostAsJsonAsync("/api/auth/register", request);

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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
        response.StatusCode.Should().Be(HttpStatusCode.OK);
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

        var loginRequest = new LoginUserRequest(Email: email, Password: "WrongPassword1");

        // Act
        var response = await Client.AsAnonymous().PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMe_ShouldReturnUnauthorized_WhenUserIsAnonymous()
    {
        // Act
        var response = await Client.AsAnonymous().GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMe_ShouldReturnProfile_WhenUserIsAuthenticated()
    {
        // Arrange - Register a real user in Postgres
        var email = $"getme_{Guid.NewGuid():N}@test.com";
        var registerRequest = new RegisterUserRequest(
            Email: email,
            Password: "Password123!",
            FullName: "Profile User",
            Phone: "0988888888",
            LanguagePreference: "en");

        var regResponse = await Client.AsAnonymous().PostAsJsonAsync("/api/auth/register", registerRequest);
        var regJson = await regResponse.Content.ReadFromJsonAsync<JsonElement>();
        var userId = regJson.GetProperty("userId").GetGuid();

        // Act
        var response = await Client.AsCustomer(userId).GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = await response.Content.ReadFromJsonAsync<JsonElement>();
        profile.GetProperty("email").GetString().Should().Be(email);
    }
}
