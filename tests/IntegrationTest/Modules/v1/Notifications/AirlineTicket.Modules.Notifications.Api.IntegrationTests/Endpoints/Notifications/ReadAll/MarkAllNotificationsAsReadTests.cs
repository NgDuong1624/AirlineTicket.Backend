using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Notifications.Api.IntegrationTests.Endpoints.Notifications.ReadAll;

[Collection("NotificationsTests")]
public class MarkAllNotificationsAsReadTests : BaseIntegrationTest
{
    public MarkAllNotificationsAsReadTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task MarkAllAsRead_ShouldReturnUnauthorized_WhenAnonymous()
    {
        // Act
        var response = await Client.AsAnonymous().PutAsync("/api/notifications/read-all", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "PUT /api/notifications/read-all: MarkAllNotificationsAsRead must return 401 Unauthorized but return {0}", response.StatusCode);
    }

    [Fact]
    public async Task MarkAllAsRead_ShouldReturnNoContent_WhenCustomer()
    {
        // Act
        var response = await Client.AsCustomer().PutAsync("/api/notifications/read-all", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent, "PUT /api/notifications/read-all: MarkAllNotificationsAsRead must return 204 NoContent but return {0}", response.StatusCode);
    }
}
