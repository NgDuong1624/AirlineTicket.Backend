using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Users.Application.Features.Admin;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Users.Api.IntegrationTests.Endpoints.Admin.Users;

[Collection("UsersTests")]
public class AdminGetUsersTests : BaseIntegrationTest
{
    public AdminGetUsersTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetUsers_ShouldReturnForbidden_WhenAnonymousOrCustomer()
    {
        // Act
        var anonResponse = await Client.AsAnonymous().GetAsync("/api/admin/users");
        var custResponse = await Client.AsCustomer().GetAsync("/api/admin/users");

        // Assert
        anonResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "GET /api/admin/users: GetUsers_ShouldReturnForbidden_WhenAnonymousOrCustomer must return 401 Unauthorized but return {0}", anonResponse.StatusCode);
        custResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden, "GET /api/admin/users: GetUsers_ShouldReturnForbidden_WhenAnonymousOrCustomer must return 403 Forbidden but return {0}", custResponse.StatusCode);
    }

    [Fact]
    public async Task GetUsers_ShouldReturnOk_WhenAdminUser()
    {
        // Act
        var response = await Client.AsAdmin().GetAsync("/api/admin/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/admin/users: GetUsers_ShouldReturnOk_WhenAdminUser must return 200 OK but return {0}", response.StatusCode);
    }
}
