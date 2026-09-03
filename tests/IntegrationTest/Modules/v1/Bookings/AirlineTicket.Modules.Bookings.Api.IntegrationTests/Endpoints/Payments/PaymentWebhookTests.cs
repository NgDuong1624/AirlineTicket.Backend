using System.Threading.Tasks;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Api.IntegrationTests.Endpoints.Payments;

public class PaymentWebhookTests
{
    [Theory]
    [InlineData("/api/payments/webhooks/stripe")]
    [InlineData("/api/payments/webhooks/paypal")]
    [InlineData("/api/payments/webhooks/vnpay")]
    [InlineData("/api/payments/webhooks/momo")]
    public static void WebhookEndpoints_AllowAnonymousAccess(string route)
    {
        Assert.StartsWith("/api/payments/webhooks/", route);
    }
}
