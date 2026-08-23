using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Api.IntegrationTests.Endpoints.Bookings.Id;

[Collection("BookingsTests")]
public class CancelBookingTests : BaseIntegrationTest
{
    public CancelBookingTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CancelBooking_ShouldReturnNotFound_WhenBookingDoesNotExist()
    {
        // Act
        var response = await Client.AsAnonymous().DeleteAsync($"/api/bookings/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "DELETE /api/bookings/{id}: CancelBooking_ShouldReturnNotFound_WhenBookingDoesNotExist must return 404 NotFound but return {0}", response.StatusCode);
    }
}
