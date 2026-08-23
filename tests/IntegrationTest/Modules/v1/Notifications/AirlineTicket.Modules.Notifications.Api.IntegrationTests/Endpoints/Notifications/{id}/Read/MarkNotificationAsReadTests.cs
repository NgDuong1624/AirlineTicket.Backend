using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Notifications.Api.IntegrationTests.Endpoints.Notifications.Id.Read;

[Collection("NotificationsTests")]
public class MarkNotificationAsReadTests : BaseIntegrationTest
{
    public MarkNotificationAsReadTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task MarkAsRead_ShouldReturnUnauthorized_WhenAnonymous()
    {
        // Act
        var response = await Client.AsAnonymous().PutAsync($"/api/notifications/{Guid.NewGuid()}/read", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "PUT /api/notifications/{{id}}/read: MarkNotificationAsRead must return 401 Unauthorized but return {0}", response.StatusCode);
    }

    [Fact]
    public async Task MarkAsRead_ShouldReturnNotFound_WhenNotificationDoesNotExist()
    {
        // Act
        var response = await Client.AsCustomer().PutAsync($"/api/notifications/{Guid.NewGuid()}/read", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "PUT /api/notifications/{{id}}/read: MarkNotificationAsRead must return 404 NotFound but return {0}", response.StatusCode);
    }
}
