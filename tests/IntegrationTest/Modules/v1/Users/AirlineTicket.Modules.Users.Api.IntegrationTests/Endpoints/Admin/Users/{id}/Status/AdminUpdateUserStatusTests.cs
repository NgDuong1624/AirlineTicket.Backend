using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Users.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Users.Api.IntegrationTests.Endpoints.Admin.Users.Id.Status;

[Collection("UsersTests")]
public class AdminUpdateUserStatusTests : BaseIntegrationTest
{
    public AdminUpdateUserStatusTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdateUserStatus_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var request = new UpdateUserStatusRequest(0);

        // Act
        var response = await Client.AsAdmin().PatchAsJsonAsync($"/api/admin/users/{Guid.NewGuid()}/status", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "GET /api/...: UpdateUserStatus_ShouldReturnNotFound_WhenUserDoesNotExist must return 404 NotFound but return {0}", response.StatusCode);
    }
}
