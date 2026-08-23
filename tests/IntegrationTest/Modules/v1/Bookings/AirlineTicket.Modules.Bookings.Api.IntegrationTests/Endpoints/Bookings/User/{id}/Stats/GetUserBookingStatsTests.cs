using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Api.IntegrationTests.Endpoints.Bookings.User.Id.Stats;

[Collection("BookingsTests")]
public class GetUserBookingStatsTests : BaseIntegrationTest
{
    public GetUserBookingStatsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetUserStats_ShouldReturnOk_WhenCustomerRequestsOwnStats()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var response = await Client.AsCustomer(userId).GetAsync($"/api/bookings/user/{userId}/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/bookings/user/{id}/stats: GetUserStats_ShouldReturnOk_WhenCustomerRequestsOwnStats must return 200 OK but return {0}", response.StatusCode);
    }
}
