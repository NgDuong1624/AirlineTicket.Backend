using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Notifications.Api.IntegrationTests.Endpoints.Notifications.Status;

[Collection("NotificationsTests")]
public class GetNotificationsStatusTests : BaseIntegrationTest
{
    public GetNotificationsStatusTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetStatus_ShouldReturnOk_WhenAnonymous()
    {
        // Act
        var response = await Client.AsAnonymous().GetAsync("/api/notifications/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/notifications/status: GetNotificationsStatus must return 200 OK but return {0}", response.StatusCode);
    }
}
