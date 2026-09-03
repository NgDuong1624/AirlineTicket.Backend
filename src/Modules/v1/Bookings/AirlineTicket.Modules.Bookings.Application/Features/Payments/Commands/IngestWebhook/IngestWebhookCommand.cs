using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Domain.Entities;
using AirlineTicket.Modules.Bookings.Domain.Enums;

namespace AirlineTicket.Modules.Bookings.Application.Features.Payments.Commands.IngestWebhook;

public sealed record IngestWebhookCommand(
    PaymentProvider Provider,
    string RawBody,
    string? SignatureHeader,
    string? EventType,
    string? TimestampHeader
) : ICommand<Result<PaymentWebhookResult>>;

public class IngestWebhookCommandHandler(
    IPaymentGatewayFactory gatewayFactory,
    IWebhookEventRepository webhookEventRepository,
    IPaymentRepository paymentRepository,
    IBookingRepository bookingRepository)
    : ICommandHandler<IngestWebhookCommand, Result<PaymentWebhookResult>>
{
    private readonly IPaymentGatewayFactory _gatewayFactory = gatewayFactory;
    private readonly IWebhookEventRepository _webhookEventRepository = webhookEventRepository;
    private readonly IPaymentRepository _paymentRepository = paymentRepository;
    private readonly IBookingRepository _bookingRepository = bookingRepository;

    public async Task<Result<PaymentWebhookResult>> Handle(IngestWebhookCommand request, CancellationToken cancellationToken)
    {
        var gateway = _gatewayFactory.GetGateway(request.Provider);
        var payload = new WebhookPayload(
            request.Provider,
            request.RawBody,
            request.SignatureHeader,
            request.EventType,
            request.TimestampHeader
        );

        var webhookResult = await gateway.ProcessWebhookAsync(payload, cancellationToken);
        if (!webhookResult.IsValid || string.IsNullOrEmpty(webhookResult.ProviderEventId))
        {
            return Result.Failure<PaymentWebhookResult>(Error.Create("Webhook.InvalidSignature", webhookResult.FailureReason ?? "Invalid webhook signature."));
        }

        // Fast-idempotency check
        if (await _webhookEventRepository.ExistsAsync(request.Provider, webhookResult.ProviderEventId, cancellationToken))
        {
            return Result.Success(webhookResult);
        }

        var webhookEvent = new WebhookEvent
        {
            Id = Guid.NewGuid(),
            Provider = request.Provider,
            ProviderEventId = webhookResult.ProviderEventId,
            Payload = request.RawBody,
            Status = WebhookEventStatus.Received,
            ReceivedAt = DateTime.UtcNow
        };

        await _webhookEventRepository.CreateAsync(webhookEvent, cancellationToken);

        // Update payment and booking if transaction found
        if (!string.IsNullOrEmpty(webhookResult.ProviderTransactionId))
        {
            var payment = await _paymentRepository.GetByProviderTransactionIdAsync(
                request.Provider,
                webhookResult.ProviderTransactionId,
                cancellationToken
            );

            if (payment != null)
            {
                payment.TransitionTo(
                    webhookResult.Status,
                    webhookResult.ProviderTimestamp,
                    webhookResult.RawJson,
                    webhookResult.FailureReason
                );
                await _paymentRepository.UpdateAsync(payment, cancellationToken);

                if (webhookResult.Status == PaymentTransactionStatus.Succeeded)
                {
                    var booking = await _bookingRepository.GetByIdAsync(payment.BookingId, cancellationToken);
                    if (booking != null)
                    {
                        booking.Status = "Confirmed";
                        await _bookingRepository.UpdateAsync(booking, cancellationToken);
                    }
                }
            }
        }

        webhookEvent.Status = WebhookEventStatus.Processed;
        webhookEvent.ProcessedAt = DateTime.UtcNow;
        await _webhookEventRepository.UpdateAsync(webhookEvent, cancellationToken);

        return Result.Success(webhookResult);
    }
}
