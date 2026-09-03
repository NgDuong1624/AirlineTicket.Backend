using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Domain.Entities;
using AirlineTicket.Modules.Bookings.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AirlineTicket.Modules.Bookings.Infrastructure.Data.Repositories;

public class PaymentRepository(BookingDbContext context) : IPaymentRepository
{
    private readonly BookingDbContext _context = context;

    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Payment?> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(p => p.BookingId == bookingId, cancellationToken);
    }

    public async Task<Payment?> GetByProviderTransactionIdAsync(PaymentProvider provider, string transactionId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.Provider == provider && p.ProviderTransactionId == transactionId, cancellationToken);
    }

    public async Task<Guid> CreateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);
        return payment.Id;
    }

    public async Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class WebhookEventRepository(BookingDbContext context) : IWebhookEventRepository
{
    private readonly BookingDbContext _context = context;

    public async Task<bool> ExistsAsync(PaymentProvider provider, string providerEventId, CancellationToken cancellationToken = default)
    {
        return await _context.WebhookEvents
            .AnyAsync(e => e.Provider == provider && e.ProviderEventId == providerEventId, cancellationToken);
    }

    public async Task<WebhookEvent?> GetByProviderEventIdAsync(PaymentProvider provider, string providerEventId, CancellationToken cancellationToken = default)
    {
        return await _context.WebhookEvents
            .FirstOrDefaultAsync(e => e.Provider == provider && e.ProviderEventId == providerEventId, cancellationToken);
    }

    public async Task<Guid> CreateAsync(WebhookEvent webhookEvent, CancellationToken cancellationToken = default)
    {
        _context.WebhookEvents.Add(webhookEvent);
        await _context.SaveChangesAsync(cancellationToken);
        return webhookEvent.Id;
    }

    public async Task UpdateAsync(WebhookEvent webhookEvent, CancellationToken cancellationToken = default)
    {
        _context.WebhookEvents.Update(webhookEvent);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
