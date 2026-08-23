using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Api.IntegrationTests.Endpoints.Admin.Bookings.Id;

[Collection("BookingsTests")]
public class AdminCancelBookingTests : BaseIntegrationTest
{
    public AdminCancelBookingTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AdminCancelBooking_ShouldReturnNotFound_WhenBookingDoesNotExist()
    {
        // Act
        var response = await Client.AsAdmin().DeleteAsync($"/api/admin/bookings/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "DELETE /api/admin/bookings/{id}: AdminCancelBooking_ShouldReturnNotFound_WhenBookingDoesNotExist must return 404 NotFound but return {0}", response.StatusCode);
    }
}
