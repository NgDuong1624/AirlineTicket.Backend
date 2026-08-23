using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Notifications.Api.IntegrationTests.Endpoints.Admin.Notifications.Templates;

[Collection("NotificationsTests")]
public class GetTemplatesTests : BaseIntegrationTest
{
    public GetTemplatesTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetTemplates_ShouldReturnForbidden_WhenCustomer()
    {
        // Act
        var response = await Client.AsCustomer().GetAsync("/api/admin/notifications/templates");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden, "GET /api/admin/notifications/templates: GetTemplates must return 403 Forbidden but return {0}", response.StatusCode);
    }

    [Fact]
    public async Task GetTemplates_ShouldReturnOk_WhenAdmin()
    {
        // Act
        var response = await Client.AsAdmin().GetAsync("/api/admin/notifications/templates");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/admin/notifications/templates: GetTemplates must return 200 OK but return {0}", response.StatusCode);
    }
}
