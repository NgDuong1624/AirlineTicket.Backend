using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Notifications.Api.IntegrationTests.Endpoints.Notifications.Id;

[Collection("NotificationsTests")]
public class DeleteNotificationTests : BaseIntegrationTest
{
    public DeleteNotificationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task DeleteNotification_ShouldReturnUnauthorized_WhenAnonymous()
    {
        // Act
        var response = await Client.AsAnonymous().DeleteAsync($"/api/notifications/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "DELETE /api/notifications/{{id}}: DeleteNotification must return 401 Unauthorized but return {0}", response.StatusCode);
    }

    [Fact]
    public async Task DeleteNotification_ShouldReturnNotFound_WhenNotificationDoesNotExist()
    {
        // Act
        var response = await Client.AsCustomer().DeleteAsync($"/api/notifications/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "DELETE /api/notifications/{{id}}: DeleteNotification must return 404 NotFound but return {0}", response.StatusCode);
    }
}
