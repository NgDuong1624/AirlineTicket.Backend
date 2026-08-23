using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Bookings.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Api.IntegrationTests.Endpoints.Bookings;

[Collection("BookingsTests")]
public class CreateBookingTests : BaseIntegrationTest
{
    public CreateBookingTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateBooking_ShouldReturnBadRequest_WhenFlightDoesNotExist()
    {
        // Arrange
        var request = new CreateBookingRequest(
            FlightId: Guid.NewGuid(),
            ContactEmail: "test_booking@test.com",
            ContactPhone: "0912345678",
            Passengers: new List<PassengerDto>
            {
                new("John", "Doe", "123456789", "12A")
            });

        // Act
        var response = await Client.AsAnonymous().PostAsJsonAsync("/api/bookings", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "POST /api/bookings: CreateBooking_ShouldReturnBadRequest_WhenFlightDoesNotExist must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
