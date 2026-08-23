using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Api.IntegrationTests.Endpoints.Bookings.Id;

[Collection("BookingsTests")]
public class GetBookingByIdTests : BaseIntegrationTest
{
    public GetBookingByIdTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetBookingById_ShouldReturnNotFound_WhenBookingDoesNotExist()
    {
        // Act
        var response = await Client.AsAnonymous().GetAsync($"/api/bookings/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "GET /api/bookings/{id}: GetBookingById_ShouldReturnNotFound_WhenBookingDoesNotExist must return 404 NotFound but return {0}", response.StatusCode);
    }
}
