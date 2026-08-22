using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Bookings.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Api.IntegrationTests;

public class BookingEndpointsTests : BaseIntegrationTest
{
    public BookingEndpointsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetBookingById_ShouldReturnNotFound_WhenBookingDoesNotExist()
    {
        // Act
        var response = await Client.AsAnonymous().GetAsync($"/api/bookings/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllBookings_ShouldReturnForbidden_WhenAnonymousOrRegularCustomer()
    {
        // Act
        var anonResponse = await Client.AsAnonymous().GetAsync("/api/bookings");
        var custResponse = await Client.AsCustomer().GetAsync("/api/bookings");

        // Assert
        anonResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        custResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllBookings_ShouldReturnOk_WhenStaffUser()
    {
        // Arrange
        var airlineId = Guid.NewGuid();

        // Act
        var response = await Client.AsStaff(airlineId).GetAsync("/api/bookings");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
