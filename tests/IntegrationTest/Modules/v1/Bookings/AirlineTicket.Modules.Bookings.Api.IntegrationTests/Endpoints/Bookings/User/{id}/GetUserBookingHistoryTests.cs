using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Api.IntegrationTests.Endpoints.Bookings.User.Id;

[Collection("BookingsTests")]
public class GetUserBookingHistoryTests : BaseIntegrationTest
{
    public GetUserBookingHistoryTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetUserBookings_ShouldReturnUnauthorized_WhenAnonymous()
    {
        // Act
        var response = await Client.AsAnonymous().GetAsync($"/api/bookings/user/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "GET /api/bookings/user/{id}: GetUserBookings_ShouldReturnUnauthorized_WhenAnonymous must return 401 Unauthorized but return {0}", response.StatusCode);
    }

    [Fact]
    public async Task GetUserBookings_ShouldReturnOk_WhenCustomerRequestsOwnBookings()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var response = await Client.AsCustomer(userId).GetAsync($"/api/bookings/user/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/bookings/user/{id}: GetUserBookings_ShouldReturnOk_WhenCustomerRequestsOwnBookings must return 200 OK but return {0}", response.StatusCode);
    }
}
