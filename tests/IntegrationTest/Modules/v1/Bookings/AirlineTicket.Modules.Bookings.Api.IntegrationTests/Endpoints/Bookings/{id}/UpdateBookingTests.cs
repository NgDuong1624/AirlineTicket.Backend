using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Bookings.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Api.IntegrationTests.Endpoints.Bookings.Id;

[Collection("BookingsTests")]
public class UpdateBookingTests : BaseIntegrationTest
{
    public UpdateBookingTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdateBooking_ShouldReturnNotFound_WhenBookingDoesNotExist()
    {
        // Arrange
        var request = new UpdateBookingRequest(
            Passengers: null,
            ContactEmail: "updated@test.com",
            ContactPhone: "0999999999");

        // Act
        var response = await Client.AsAnonymous().PutAsJsonAsync($"/api/bookings/{Guid.NewGuid()}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "PUT /api/bookings/{id}: UpdateBooking_ShouldReturnNotFound_WhenBookingDoesNotExist must return 404 NotFound but return {0}", response.StatusCode);
    }
}
