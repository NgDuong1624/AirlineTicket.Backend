using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Notifications.Api.IntegrationTests.Endpoints.Notifications;

[Collection("NotificationsTests")]
public class GetUserNotificationsTests : BaseIntegrationTest
{
    public GetUserNotificationsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetUserNotifications_ShouldReturnUnauthorized_WhenAnonymous()
    {
        // Act
        var response = await Client.AsAnonymous().GetAsync("/api/notifications");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "GET /api/notifications: GetUserNotifications must return 401 Unauthorized but return {0}", response.StatusCode);
    }

    [Fact]
    public async Task GetUserNotifications_ShouldReturnOk_WhenCustomer()
    {
        // Act
        var response = await Client.AsCustomer().GetAsync("/api/notifications");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/notifications: GetUserNotifications must return 200 OK but return {0}", response.StatusCode);
    }
}
