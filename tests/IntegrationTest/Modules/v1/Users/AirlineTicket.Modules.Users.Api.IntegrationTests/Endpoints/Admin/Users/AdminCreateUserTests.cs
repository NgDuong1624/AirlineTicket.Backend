using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Users.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Users.Api.IntegrationTests.Endpoints.Admin.Users;

[Collection("UsersTests")]
public class AdminCreateUserTests : BaseIntegrationTest
{
    public AdminCreateUserTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateUser_ShouldReturnForbidden_WhenNotAdmin()
    {
        // Arrange
        var request = new AdminUserRequest(
            Email: $"admin_created_{Guid.NewGuid():N}@test.com",
            FullName: "Admin Created User",
            Phone: "0911223344",
            RoleId: 2,
            IsActive: true,
            Password: "Password123!",
            AirlineId: null,
            LanguagePreference: "en");

        // Act
        var response = await Client.AsCustomer().PostAsJsonAsync("/api/admin/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden, "POST /api/admin/users: CreateUser_ShouldReturnForbidden_WhenNotAdmin must return 403 Forbidden but return {0}", response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_ShouldSucceed_WhenAdmin()
    {
        // Arrange
        var request = new AdminUserRequest(
            Email: $"admin_created_{Guid.NewGuid():N}@test.com",
            FullName: "Admin Created User",
            Phone: "0911223344",
            RoleId: 2,
            IsActive: true,
            Password: "Password123!",
            AirlineId: null,
            LanguagePreference: "en");

        // Act
        var response = await Client.AsAdmin().PostAsJsonAsync("/api/admin/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created, "POST /api/admin/users: CreateUser_ShouldSucceed_WhenAdmin must return 201 Created but return {0}", response.StatusCode);
    }
}
