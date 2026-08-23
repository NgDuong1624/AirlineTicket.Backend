using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Users.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Users.Api.IntegrationTests.Endpoints.Auth.Register;

[Collection("UsersTests")]
public class RegisterTests : BaseIntegrationTest
{
    public RegisterTests(CustomWebApplicationFactory factory) : base(factory)
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
        response.StatusCode.Should().Be(HttpStatusCode.OK, "POST /api/auth/register: Register_ShouldSucceed_WhenRequestIsValid must return 200 OK but return {0}", response.StatusCode);
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
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK, "POST /api/auth/register: Register_ShouldFail_WhenEmailAlreadyExists must return 200 OK but return {0}", firstResponse.StatusCode);

        // Act
        var secondResponse = await Client.AsAnonymous().PostAsJsonAsync("/api/auth/register", request);

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "POST /api/auth/register: Register_ShouldFail_WhenEmailAlreadyExists must return 400 BadRequest but return {0}", secondResponse.StatusCode);
    }
}
