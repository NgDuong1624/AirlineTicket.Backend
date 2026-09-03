using AirlineTicket.Modules.Bookings.Domain.Enums;

namespace AirlineTicket.Modules.Bookings.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookingId { get; set; }
    public PaymentProvider Provider { get; set; }
    public string ProviderTransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public Currency Currency { get; set; } = Currency.USD;
    public PaymentTransactionStatus Status { get; set; } = PaymentTransactionStatus.Pending;
    public DateTime? ProviderTimestamp { get; set; }
    public int ConcurrencyVersion { get; set; }
    public string? RawResponse { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Booking Booking { get; set; } = null!;

    public bool CanTransitionTo(PaymentTransactionStatus targetStatus)
    {
        return (Status, targetStatus) switch
        {
            (PaymentTransactionStatus.Pending, PaymentTransactionStatus.Processing) => true,
            (PaymentTransactionStatus.Pending, PaymentTransactionStatus.Succeeded) => true,
            (PaymentTransactionStatus.Pending, PaymentTransactionStatus.Failed) => true,
            (PaymentTransactionStatus.Pending, PaymentTransactionStatus.Cancelled) => true,
            (PaymentTransactionStatus.Processing, PaymentTransactionStatus.Succeeded) => true,
            (PaymentTransactionStatus.Processing, PaymentTransactionStatus.Failed) => true,
            (PaymentTransactionStatus.Processing, PaymentTransactionStatus.Cancelled) => true,
            (PaymentTransactionStatus.Succeeded, PaymentTransactionStatus.RefundPending) => true,
            (PaymentTransactionStatus.Succeeded, PaymentTransactionStatus.Refunded) => true,
            (PaymentTransactionStatus.RefundPending, PaymentTransactionStatus.Refunded) => true,
            _ => false
        };
    }

    public void TransitionTo(PaymentTransactionStatus targetStatus, DateTime? providerTimestamp = null, string? rawResponse = null, string? failureReason = null)
    {
        if (Status == targetStatus)
        {
            return;
        }

        if (!CanTransitionTo(targetStatus))
        {
            throw new InvalidOperationException($"Invalid status transition from {Status} to {targetStatus}.");
        }

        if (providerTimestamp.HasValue && ProviderTimestamp.HasValue && providerTimestamp < ProviderTimestamp)
        {
            // Drop out-of-order stale updates
            return;
        }

        Status = targetStatus;
        if (providerTimestamp.HasValue)
        {
            ProviderTimestamp = providerTimestamp;
        }
        if (rawResponse != null)
        {
            RawResponse = rawResponse;
        }
        if (failureReason != null)
        {
            FailureReason = failureReason;
        }
        UpdatedAt = DateTime.UtcNow;
    }
}
