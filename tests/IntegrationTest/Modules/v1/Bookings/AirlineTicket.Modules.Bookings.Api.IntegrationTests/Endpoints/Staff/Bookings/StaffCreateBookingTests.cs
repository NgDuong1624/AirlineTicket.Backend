using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Bookings.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Api.IntegrationTests.Endpoints.Staff.Bookings;

[Collection("BookingsTests")]
public class StaffCreateBookingTests : BaseIntegrationTest
{
    public StaffCreateBookingTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task StaffCreateBooking_ShouldReturnForbidden_WhenNotStaff()
    {
        // Arrange
        var request = new StaffCreateBookingRequest(
            FlightId: Guid.NewGuid(),
            ContactName: "Call In Customer",
            ContactEmail: "callin@test.com",
            ContactPhone: "0912345678",
            Passengers: new List<StaffBookingPassenger>
            {
                new("Jane", "Doe", "987654321", "14B")
            });

        // Act
        var response = await Client.AsCustomer().PostAsJsonAsync("/api/staff/bookings", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden, "POST /api/staff/bookings: StaffCreateBooking_ShouldReturnForbidden_WhenNotStaff must return 403 Forbidden but return {0}", response.StatusCode);
    }
}
