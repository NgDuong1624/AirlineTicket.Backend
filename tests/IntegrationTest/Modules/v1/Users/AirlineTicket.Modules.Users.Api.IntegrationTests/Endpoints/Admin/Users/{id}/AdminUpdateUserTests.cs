using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Users.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Users.Api.IntegrationTests.Endpoints.Admin.Users.Id;

[Collection("UsersTests")]
public class AdminUpdateUserTests : BaseIntegrationTest
{
    public AdminUpdateUserTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var request = new AdminUserRequest(
            Email: "update@test.com",
            FullName: "Updated User",
            Phone: "0911223344",
            RoleId: 2,
            IsActive: true,
            Password: "Password123!",
            AirlineId: null,
            LanguagePreference: "en");

        // Act
        var response = await Client.AsAdmin().PutAsJsonAsync($"/api/admin/users/{Guid.NewGuid()}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "PUT /api/admin/users/{Guid.NewGuid()}: UpdateUser_ShouldReturnNotFound_WhenUserDoesNotExist must return 404 NotFound but return {0}", response.StatusCode);
    }
}
