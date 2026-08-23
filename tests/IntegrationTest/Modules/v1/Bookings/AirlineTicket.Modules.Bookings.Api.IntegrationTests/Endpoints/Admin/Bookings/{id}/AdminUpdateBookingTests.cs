using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Bookings.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Api.IntegrationTests.Endpoints.Admin.Bookings.Id;

[Collection("BookingsTests")]
public class AdminUpdateBookingTests : BaseIntegrationTest
{
    public AdminUpdateBookingTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AdminUpdateBooking_ShouldReturnNotFound_WhenBookingDoesNotExist()
    {
        // Arrange
        var request = new UpdateBookingRequest(
            Passengers: null,
            ContactEmail: "admin_upd@test.com",
            ContactPhone: "0912345678");

        // Act
        var response = await Client.AsAdmin().PutAsJsonAsync($"/api/admin/bookings/{Guid.NewGuid()}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "PUT /api/admin/bookings/{id}: AdminUpdateBooking_ShouldReturnNotFound_WhenBookingDoesNotExist must return 404 NotFound but return {0}", response.StatusCode);
    }
}
