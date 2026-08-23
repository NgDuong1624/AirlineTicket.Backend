using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Users.Api.IntegrationTests.Endpoints.Admin.Users.Permissions;

[Collection("UsersTests")]
public class AdminGetPermissionsTests : BaseIntegrationTest
{
    public AdminGetPermissionsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetPermissions_ShouldReturnOk_WhenAdmin()
    {
        // Act
        var response = await Client.AsAdmin().GetAsync("/api/admin/users/permissions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/admin/users/permissions: GetPermissions_ShouldReturnOk_WhenAdmin must return 200 OK but return {0}", response.StatusCode);
    }
}
