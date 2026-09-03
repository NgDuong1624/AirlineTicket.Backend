using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Bookings.Domain.Entities;
using AirlineTicket.Modules.Bookings.Domain.Enums;

namespace AirlineTicket.Modules.Bookings.Application.Contracts;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Payment?> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<Payment?> GetByProviderTransactionIdAsync(PaymentProvider provider, string transactionId, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(Payment payment, CancellationToken cancellationToken = default);
    Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default);
}

public interface IWebhookEventRepository
{
    Task<bool> ExistsAsync(PaymentProvider provider, string providerEventId, CancellationToken cancellationToken = default);
    Task<WebhookEvent?> GetByProviderEventIdAsync(PaymentProvider provider, string providerEventId, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(WebhookEvent webhookEvent, CancellationToken cancellationToken = default);
    Task UpdateAsync(WebhookEvent webhookEvent, CancellationToken cancellationToken = default);
}
