using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Notifications.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Notifications.Api.IntegrationTests.Endpoints.Admin.Notifications.Templates.Id;

[Collection("NotificationsTests")]
public class UpdateTemplateTests : BaseIntegrationTest
{
    public UpdateTemplateTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdateTemplate_ShouldReturnBadRequest_WhenInvalidLanguage()
    {
        // Arrange
        var request = new UpdateTemplateRequest("Subject", "Body template {{Name}}", "invalid_lang");

        // Act
        var response = await Client.AsAdmin().PutAsJsonAsync($"/api/admin/notifications/templates/{Guid.NewGuid()}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "PUT /api/admin/notifications/templates/{{id}}: UpdateTemplate must return 400 BadRequest but return {0}", response.StatusCode);
    }

    [Fact]
    public async Task UpdateTemplate_ShouldReturnNotFound_WhenTemplateDoesNotExist()
    {
        // Arrange
        var request = new UpdateTemplateRequest("Booking Confirmed", "Your booking {{Pnr}} is confirmed", "en");

        // Act
        var response = await Client.AsAdmin().PutAsJsonAsync($"/api/admin/notifications/templates/{Guid.NewGuid()}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "PUT /api/admin/notifications/templates/{{id}}: UpdateTemplate must return 404 NotFound but return {0}", response.StatusCode);
    }
}
