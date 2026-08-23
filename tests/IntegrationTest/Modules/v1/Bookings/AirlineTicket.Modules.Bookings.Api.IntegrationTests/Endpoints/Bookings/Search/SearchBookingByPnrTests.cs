using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Api.IntegrationTests.Endpoints.Bookings.Search;

[Collection("BookingsTests")]
public class SearchBookingByPnrTests : BaseIntegrationTest
{
    public SearchBookingByPnrTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task SearchBooking_ShouldReturnNotFound_WhenPnrDoesNotExist()
    {
        // Act
        var response = await Client.AsAnonymous().GetAsync("/api/bookings/search?pnrCode=NONEXISTENT123");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "GET /api/bookings/search: SearchBooking_ShouldReturnNotFound_WhenPnrDoesNotExist must return 404 NotFound but return {0}", response.StatusCode);
    }
}
