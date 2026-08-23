using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Bookings.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Api.IntegrationTests.Endpoints.Bookings.Id.Pay;

[Collection("BookingsTests")]
public class ProcessBookingPaymentTests : BaseIntegrationTest
{
    public ProcessBookingPaymentTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task PayBooking_ShouldReturnBadRequest_WhenBookingDoesNotExist()
    {
        // Arrange
        var request = new PayBookingRequest("VnPay", 500000m);

        // Act
        var response = await Client.AsAnonymous().PostAsJsonAsync($"/api/bookings/{Guid.NewGuid()}/pay", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "POST /api/bookings/{id}/pay: PayBooking_ShouldReturnBadRequest_WhenBookingDoesNotExist must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
