using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Domain.Enums;

namespace AirlineTicket.Modules.Bookings.Infrastructure.Gateways;

public class SandboxPaymentGateway : IPaymentGateway
{
    public PaymentProvider Provider => PaymentProvider.Sandbox;

    public Task<CreatePaymentResult> CreatePaymentUrlAsync(PaymentCheckoutRequest request, CancellationToken cancellationToken = default)
    {
        var txnId = $"SBX-{Guid.NewGuid():N}";
        var separator = request.ReturnUrl.Contains('?') ? "&" : "?";
        var paymentUrl = $"{request.ReturnUrl}{separator}status=success&provider=sandbox&txnRef={txnId}";
        var qrCodeUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=300x300&data={Uri.EscapeDataString(paymentUrl)}";

        return Task.FromResult(new CreatePaymentResult(
            ProviderTransactionId: txnId,
            Provider: Provider,
            PaymentUrl: paymentUrl,
            ClientSecret: null,
            OrderId: txnId,
            QrCodeUrl: qrCodeUrl
        ));
    }

    public Task<PaymentWebhookResult> ProcessWebhookAsync(WebhookPayload payload, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new PaymentWebhookResult(
            IsValid: true,
            ProviderEventId: Guid.NewGuid().ToString(),
            ProviderTransactionId: null,
            Status: PaymentTransactionStatus.Succeeded,
            FailureReason: null,
            ProviderTimestamp: DateTime.UtcNow,
            RawJson: payload.RawBody
        ));
    }

    public Task<RefundResult> RefundPaymentAsync(RefundRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new RefundResult(true, $"REF-{Guid.NewGuid():N}", null));
    }

    public Task<PaymentStatusResult> QueryTransactionStatusAsync(string providerTransactionId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new PaymentStatusResult(
            ProviderTransactionId: providerTransactionId,
            Status: PaymentTransactionStatus.Succeeded,
            FailureReason: null,
            Timestamp: DateTime.UtcNow
        ));
    }
}
