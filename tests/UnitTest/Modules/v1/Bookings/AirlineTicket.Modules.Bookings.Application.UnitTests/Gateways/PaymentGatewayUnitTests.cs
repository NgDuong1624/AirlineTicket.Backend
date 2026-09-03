using System.Net.Http;
using System.Threading.Tasks;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Domain.Enums;
using AirlineTicket.Modules.Bookings.Domain.ValueObjects;
using AirlineTicket.Modules.Bookings.Infrastructure.Gateways;
using AirlineTicket.Modules.Bookings.Infrastructure.Gateways.Options;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace AirlineTicket.Modules.Bookings.UnitTest.Gateways;

public class PaymentGatewayUnitTests
{
    [Fact]
    public async Task VNPayGateway_CreatePaymentUrl_GeneratesSignedUrlWithChecksum()
    {
        var options = Options.Create(new PaymentGatewayOptions
        {
            VNPay = new VNPayOptions
            {
                TmnCode = "TESTTMN",
                HashSecret = "TESTSECRET",
                BaseUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html"
            }
        });

        var gateway = new VNPayPaymentGateway(new HttpClient(), options, NullLogger<VNPayPaymentGateway>.Instance);
        var request = new PaymentCheckoutRequest(
            BookingId: Guid.NewGuid().ToString(),
            Money: new Money(500000m, Currency.VND),
            ReturnUrl: "https://localhost:3000/callback",
            CancelUrl: "https://localhost:3000/cancel",
            Description: "Flight Ticket Booking",
            CustomerEmail: "test@example.com"
        );

        var result = await gateway.CreatePaymentUrlAsync(request);

        Assert.NotNull(result.PaymentUrl);
        Assert.Contains("vnp_SecureHash=", result.PaymentUrl);
        Assert.Contains("vnp_TmnCode=TESTTMN", result.PaymentUrl);
        Assert.Equal(PaymentProvider.VNPay, result.Provider);
    }

    [Fact]
    public async Task MoMoGateway_ProcessWebhook_InvalidSignature_ReturnsInvalidResult()
    {
        var options = Options.Create(new PaymentGatewayOptions
        {
            MoMo = new MoMoOptions
            {
                PartnerCode = "MOMO",
                AccessKey = "ACCESS",
                SecretKey = "SECRET"
            }
        });

        var gateway = new MoMoPaymentGateway(new HttpClient(), options, NullLogger<MoMoPaymentGateway>.Instance);
        var rawPayload = """
        {
            "partnerCode": "MOMO",
            "orderId": "ORD_123",
            "requestId": "REQ_123",
            "amount": 100000,
            "orderInfo": "Ticket",
            "orderType": "momo_wallet",
            "transId": 12345678,
            "resultCode": 0,
            "message": "Success",
            "payType": "qr",
            "responseTime": 1600000000,
            "extraData": "",
            "signature": "invalidsignaturehash"
        }
        """;

        var payload = new WebhookPayload(PaymentProvider.MoMo, rawPayload, null, null, null);
        var result = await gateway.ProcessWebhookAsync(payload);

        Assert.False(result.IsValid);
        Assert.Equal(PaymentTransactionStatus.Failed, result.Status);
    }
}
