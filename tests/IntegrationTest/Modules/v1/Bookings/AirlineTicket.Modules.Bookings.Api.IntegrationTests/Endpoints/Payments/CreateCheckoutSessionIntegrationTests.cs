using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Bookings.Api.Endpoints;
using AirlineTicket.Modules.Bookings.Domain.Enums;
using FluentAssertions;

namespace AirlineTicket.Modules.Bookings.Api.IntegrationTests.Endpoints.Payments;

[Collection("BookingsTests")]
public class CreateCheckoutSessionIntegrationTests(CustomWebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task CreateCheckoutSession_ShouldReturnNotFound_WhenBookingDoesNotExist()
    {
        // Arrange
        var request = new CheckoutRequest(
            BookingId: Guid.NewGuid(),
            Provider: PaymentProvider.VNPay,
            Currency: Currency.VND,
            ReturnUrl: "https://localhost:3000/booking/payment/callback",
            CancelUrl: "https://localhost:3000/booking/payment"
        );

        // Act
        var response = await Client.AsAnonymous().PostAsJsonAsync("/api/payments/checkout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "POST /api/payments/checkout: must return 404 NotFound when booking does not exist but returned {0}", response.StatusCode);
    }
}
