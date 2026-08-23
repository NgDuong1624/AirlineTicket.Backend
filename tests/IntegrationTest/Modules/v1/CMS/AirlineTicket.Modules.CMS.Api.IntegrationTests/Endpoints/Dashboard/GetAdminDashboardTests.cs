using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.CMS.Api.IntegrationTests.Endpoints.Dashboard;

[Collection("CMSTests")]
public class GetAdminDashboardTests : BaseIntegrationTest
{
    public GetAdminDashboardTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AdminGetDashboard_ShouldReturnUnauthorized_WhenAnonymous()
    {
        // Act
        var response = await Client.AsAnonymous().GetAsync("/api/admin/dashboard");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "GET /api/admin/dashboard: AdminGetDashboard must return 401 Unauthorized but return {0}", response.StatusCode);
    }

    [Fact]
    public async Task AdminGetDashboard_ShouldReturnOk_WhenAdmin()
    {
        // Act
        var response = await Client.AsAdmin().GetAsync("/api/admin/dashboard");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/admin/dashboard: AdminGetDashboard must return 200 OK but return {0}", response.StatusCode);
    }
}
