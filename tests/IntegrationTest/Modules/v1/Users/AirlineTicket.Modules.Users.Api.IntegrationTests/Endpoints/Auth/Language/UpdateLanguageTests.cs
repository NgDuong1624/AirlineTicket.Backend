using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Users.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Users.Api.IntegrationTests.Endpoints.Auth.Language;

[Collection("UsersTests")]
public class UpdateLanguageTests : BaseIntegrationTest
{
    public UpdateLanguageTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdateLanguage_ShouldReturnUnauthorized_WhenAnonymous()
    {
        // Act
        var response = await Client.AsAnonymous().PutAsJsonAsync("/api/auth/language", new UpdateLanguageRequest("vi"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "PUT /api/auth/language: UpdateLanguage_ShouldReturnUnauthorized_WhenAnonymous must return 401 Unauthorized but return {0}", response.StatusCode);
    }

    [Fact]
    public async Task UpdateLanguage_ShouldSucceed_WhenAuthenticated()
    {
        // Arrange
        var email = $"lang_{Guid.NewGuid():N}@test.com";
        var registerRequest = new RegisterUserRequest(
            Email: email,
            Password: "Password123!",
            FullName: "Lang Test User",
            Phone: "0933333333",
            LanguagePreference: "en");

        var regResponse = await Client.AsAnonymous().PostAsJsonAsync("/api/auth/register", registerRequest);
        var regContent = await regResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var userId = regContent.GetProperty("userId").GetGuid();

        // Act
        var response = await Client.AsCustomer(userId).PutAsJsonAsync("/api/auth/language", new UpdateLanguageRequest("vi"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "PUT /api/auth/language: UpdateLanguage_ShouldSucceed_WhenAuthenticated must return 200 OK but return {0}", response.StatusCode);
    }
}
