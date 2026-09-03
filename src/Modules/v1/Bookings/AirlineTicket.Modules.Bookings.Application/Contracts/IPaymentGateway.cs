using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Bookings.Domain.Enums;
using AirlineTicket.Modules.Bookings.Domain.ValueObjects;

namespace AirlineTicket.Modules.Bookings.Application.Contracts;

public sealed record PaymentCheckoutRequest(
    string BookingId,
    Money Money,
    string ReturnUrl,
    string CancelUrl,
    string Description,
    string CustomerEmail,
    string? CustomerName = null
);

public sealed record CreatePaymentResult(
    string ProviderTransactionId,
    PaymentProvider Provider,
    string? PaymentUrl,
    string? ClientSecret,
    string? OrderId
);

public sealed record WebhookPayload(
    PaymentProvider Provider,
    string RawBody,
    string? SignatureHeader,
    string? EventType,
    string? TimestampHeader
);

public sealed record PaymentWebhookResult(
    bool IsValid,
    string? ProviderEventId,
    string? ProviderTransactionId,
    PaymentTransactionStatus Status,
    string? FailureReason,
    System.DateTime? ProviderTimestamp,
    string? RawJson
);

public sealed record RefundRequest(
    string ProviderTransactionId,
    Money Money,
    string Reason
);

public sealed record RefundResult(
    bool IsSuccess,
    string? RefundTransactionId,
    string? FailureReason
);

public sealed record PaymentStatusResult(
    string ProviderTransactionId,
    PaymentTransactionStatus Status,
    string? FailureReason,
    System.DateTime? Timestamp
);

public interface IPaymentGateway
{
    PaymentProvider Provider { get; }
    Task<CreatePaymentResult> CreatePaymentUrlAsync(PaymentCheckoutRequest request, CancellationToken cancellationToken = default);
    Task<PaymentWebhookResult> ProcessWebhookAsync(WebhookPayload payload, CancellationToken cancellationToken = default);
    Task<RefundResult> RefundPaymentAsync(RefundRequest request, CancellationToken cancellationToken = default);
    Task<PaymentStatusResult> QueryTransactionStatusAsync(string providerTransactionId, CancellationToken cancellationToken = default);
}

public interface IPaymentGatewayFactory
{
    IPaymentGateway GetGateway(PaymentProvider provider);
}
