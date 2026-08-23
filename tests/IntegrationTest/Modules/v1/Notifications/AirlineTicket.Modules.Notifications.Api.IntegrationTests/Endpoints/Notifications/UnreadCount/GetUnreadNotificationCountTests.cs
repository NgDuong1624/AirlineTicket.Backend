using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Notifications.Api.IntegrationTests.Endpoints.Notifications.UnreadCount;

[Collection("NotificationsTests")]
public class GetUnreadNotificationCountTests : BaseIntegrationTest
{
    public GetUnreadNotificationCountTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetUnreadCount_ShouldReturnUnauthorized_WhenAnonymous()
    {
        // Act
        var response = await Client.AsAnonymous().GetAsync("/api/notifications/unread-count");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "GET /api/notifications/unread-count: GetUnreadNotificationCount must return 401 Unauthorized but return {0}", response.StatusCode);
    }

    [Fact]
    public async Task GetUnreadCount_ShouldReturnOk_WhenCustomer()
    {
        // Act
        var response = await Client.AsCustomer().GetAsync("/api/notifications/unread-count");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/notifications/unread-count: GetUnreadNotificationCount must return 200 OK but return {0}", response.StatusCode);
    }
}
