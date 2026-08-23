using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Api.IntegrationTests.Endpoints.Bookings;

[Collection("BookingsTests")]
public class GetAllBookingsTests : BaseIntegrationTest
{
    public GetAllBookingsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetAllBookings_ShouldReturnForbidden_WhenAnonymousOrCustomer()
    {
        // Act
        var anon = await Client.AsAnonymous().GetAsync("/api/bookings");
        var cust = await Client.AsCustomer().GetAsync("/api/bookings");

        // Assert
        anon.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "GET /api/bookings: GetAllBookings_ShouldReturnForbidden_WhenAnonymousOrCustomer must return 401 Unauthorized but return {0}", anon.StatusCode);
        cust.StatusCode.Should().Be(HttpStatusCode.Forbidden, "GET /api/bookings: GetAllBookings_ShouldReturnForbidden_WhenAnonymousOrCustomer must return 403 Forbidden but return {0}", cust.StatusCode);
    }

    [Fact]
    public async Task GetAllBookings_ShouldReturnOk_WhenStaffUser()
    {
        // Arrange
        var airlineId = Guid.NewGuid();

        // Act
        var response = await Client.AsStaff(airlineId).GetAsync("/api/bookings");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/bookings: GetAllBookings_ShouldReturnOk_WhenStaffUser must return 200 OK but return {0}", response.StatusCode);
    }
}
