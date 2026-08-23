using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Notifications.Api.IntegrationTests.Endpoints.Admin.Notifications.Templates.Id;

[Collection("NotificationsTests")]
public class GetTemplateByIdTests : BaseIntegrationTest
{
    public GetTemplateByIdTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetTemplateById_ShouldReturnForbidden_WhenCustomer()
    {
        // Act
        var response = await Client.AsCustomer().GetAsync($"/api/admin/notifications/templates/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden, "GET /api/admin/notifications/templates/{{id}}: GetTemplateById must return 403 Forbidden but return {0}", response.StatusCode);
    }

    [Fact]
    public async Task GetTemplateById_ShouldReturnNotFound_WhenTemplateDoesNotExist()
    {
        // Act
        var response = await Client.AsAdmin().GetAsync($"/api/admin/notifications/templates/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "GET /api/admin/notifications/templates/{{id}}: GetTemplateById must return 404 NotFound but return {0}", response.StatusCode);
    }
}
