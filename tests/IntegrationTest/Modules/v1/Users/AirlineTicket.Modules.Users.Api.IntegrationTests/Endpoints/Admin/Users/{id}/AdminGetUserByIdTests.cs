using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Users.Application.Features.Admin;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Users.Api.IntegrationTests.Endpoints.Admin.Users.Id;

[Collection("UsersTests")]
public class AdminGetUserByIdTests : BaseIntegrationTest
{
    public AdminGetUserByIdTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetUserById_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Act
        var response = await Client.AsAdmin().GetAsync($"/api/admin/users/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "GET /api/admin/users/{Guid.NewGuid()}: GetUserById_ShouldReturnNotFound_WhenUserDoesNotExist must return 404 NotFound but return {0}", response.StatusCode);
    }
}
