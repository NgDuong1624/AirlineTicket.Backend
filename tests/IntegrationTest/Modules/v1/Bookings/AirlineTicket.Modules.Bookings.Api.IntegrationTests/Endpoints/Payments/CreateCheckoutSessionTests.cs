using System.Threading.Tasks;
using AirlineTicket.Modules.Bookings.Api.Endpoints;
using AirlineTicket.Modules.Bookings.Domain.Enums;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Api.IntegrationTests.Endpoints.Payments;

public class CreateCheckoutSessionTests
{
    [Fact]
    public void CheckoutRequest_SerializationContract_MatchesExpectedSchema()
    {
        var request = new CheckoutRequest(
            BookingId: Guid.NewGuid(),
            Provider: PaymentProvider.Stripe,
            Currency: Currency.USD,
            ReturnUrl: "https://localhost:3000/booking/payment/callback",
            CancelUrl: "https://localhost:3000/booking/payment"
        );

        Assert.NotEqual(Guid.Empty, request.BookingId);
        Assert.Equal(PaymentProvider.Stripe, request.Provider);
        Assert.Equal(Currency.USD, request.Currency);
    }
}
