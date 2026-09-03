namespace AirlineTicket.Modules.Bookings.Infrastructure.Gateways.Options;

public class PaymentGatewayOptions
{
    public const string SectionName = "Payment";

    public StripeOptions Stripe { get; set; } = new();
    public PayPalOptions PayPal { get; set; } = new();
    public VNPayOptions VNPay { get; set; } = new();
    public MoMoOptions MoMo { get; set; } = new();
}

public class StripeOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public string PublishableKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
}

public class PayPalOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string WebhookId { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api-m.sandbox.paypal.com";
}

public class VNPayOptions
{
    public string TmnCode { get; set; } = string.Empty;
    public string HashSecret { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
    public string QueryDrUrl { get; set; } = "https://sandbox.vnpayment.vn/merchant_webapi/api/transaction";
    public string Version { get; set; } = "2.1.0";
    public string Command { get; set; } = "pay";
}

public class MoMoOptions
{
    public string PartnerCode { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string Endpoint { get; set; } = "https://test-payment.momo.vn/v2/gateway/api/create";
    public string QueryEndpoint { get; set; } = "https://test-payment.momo.vn/v2/gateway/api/query";
    public string RefundEndpoint { get; set; } = "https://test-payment.momo.vn/v2/gateway/api/refund";
}
