using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Api.IntegrationTests.Endpoints.Admin.Bookings.Id;

[Collection("BookingsTests")]
public class AdminGetBookingByIdTests : BaseIntegrationTest
{
    public AdminGetBookingByIdTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AdminGetBookingById_ShouldReturnNotFound_WhenBookingDoesNotExist()
    {
        // Act
        var response = await Client.AsAdmin().GetAsync($"/api/admin/bookings/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "GET /api/admin/bookings/{id}: AdminGetBookingById_ShouldReturnNotFound_WhenBookingDoesNotExist must return 404 NotFound but return {0}", response.StatusCode);
    }
}
