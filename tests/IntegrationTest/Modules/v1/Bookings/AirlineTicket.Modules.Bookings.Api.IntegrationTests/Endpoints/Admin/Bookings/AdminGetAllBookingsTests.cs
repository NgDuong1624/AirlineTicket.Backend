using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Api.IntegrationTests.Endpoints.Admin.Bookings;

[Collection("BookingsTests")]
public class AdminGetAllBookingsTests : BaseIntegrationTest
{
    public AdminGetAllBookingsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AdminGetBookings_ShouldReturnForbidden_WhenCustomer()
    {
        // Act
        var response = await Client.AsCustomer().GetAsync("/api/admin/bookings");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden, "GET /api/admin/bookings: AdminGetBookings_ShouldReturnForbidden_WhenCustomer must return 403 Forbidden but return {0}", response.StatusCode);
    }

    [Fact]
    public async Task AdminGetBookings_ShouldReturnOk_WhenAdmin()
    {
        // Act
        var response = await Client.AsAdmin().GetAsync("/api/admin/bookings");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/admin/bookings: AdminGetBookings_ShouldReturnOk_WhenAdmin must return 200 OK but return {0}", response.StatusCode);
    }
}
