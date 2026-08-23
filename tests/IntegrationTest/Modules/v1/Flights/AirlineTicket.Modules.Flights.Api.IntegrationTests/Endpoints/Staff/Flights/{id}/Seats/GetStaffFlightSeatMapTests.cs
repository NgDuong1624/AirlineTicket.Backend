using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Staff.Flights.Id.Seats;

[Collection("FlightsTests")]
public class GetStaffFlightSeatMapTests : BaseIntegrationTest
{
    public GetStaffFlightSeatMapTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetStaffFlightSeats_ShouldReturnForbidden_WhenCustomer()
    {
        // Act
        var response = await Client.AsCustomer().GetAsync($"/api/staff/flights/{Guid.NewGuid()}/seats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden, "GET /api/staff/flights/{Guid.NewGuid()}/seats: GetStaffFlightSeats_ShouldReturnForbidden_WhenCustomer must return 403 Forbidden but return {0}", response.StatusCode);
    }
}
