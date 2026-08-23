using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Users.Api.IntegrationTests.Endpoints.Admin.Users.Id;

[Collection("UsersTests")]
public class AdminDeleteUserTests : BaseIntegrationTest
{
    public AdminDeleteUserTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Act
        var response = await Client.AsAdmin().DeleteAsync($"/api/admin/users/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "DELETE /api/admin/users/{Guid.NewGuid()}: DeleteUser_ShouldReturnNotFound_WhenUserDoesNotExist must return 404 NotFound but return {0}", response.StatusCode);
    }
}
