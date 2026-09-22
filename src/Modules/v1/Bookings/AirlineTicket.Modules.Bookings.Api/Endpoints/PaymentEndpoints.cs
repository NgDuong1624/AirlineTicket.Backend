using System;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.BuildingBlocks.Api.Extensions;
using AirlineTicket.BuildingBlocks.Domain.Constants;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Application.Features.Payments.Commands.CreateCheckoutSession;
using AirlineTicket.Modules.Bookings.Application.Features.Payments.Commands.IngestWebhook;
using AirlineTicket.Modules.Bookings.Application.Features.Payments.Queries.GetPaymentStatus;
using AirlineTicket.Modules.Bookings.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AirlineTicket.Modules.Bookings.Api.Endpoints;

public class PaymentEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payments")
            .WithTags("Payments Module");

        // POST /api/payments/checkout — Initiate payment session
        group.MapPost("/checkout", async (
                [FromBody] CheckoutRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new CreateCheckoutSessionCommand(
                    BookingId: request.BookingId,
                    Provider: request.Provider,
                    Currency: request.Currency,
                    ReturnUrl: request.ReturnUrl,
                    CancelUrl: request.CancelUrl
                );

                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult();
            })
            .WithName("CreateCheckoutSession")
            .WithSummary("Initiate a multi-gateway checkout session")
            .Produces(200)
            .Produces(400)
            .AllowAnonymous();

        // GET /api/payments/{bookingId:guid}/status — Query payment status
        group.MapGet("/{bookingId:guid}/status", async (
                Guid bookingId,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetPaymentStatusQuery(bookingId);
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult(statusCode: 404);
            })
            .WithName("GetPaymentStatus")
            .WithSummary("Get payment transaction status for booking")
            .Produces(200)
            .Produces(404)
            .AllowAnonymous();

        // POST /api/payments/webhooks/stripe
        group.MapPost("/webhooks/stripe", async (
                HttpRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var rawBody = await ReadRawBodyAsync(request);
                var sigHeader = request.Headers["Stripe-Signature"].ToString();

                var command = new IngestWebhookCommand(
                    Provider: PaymentProvider.Stripe,
                    RawBody: rawBody,
                    SignatureHeader: sigHeader,
                    EventType: null,
                    TimestampHeader: null
                );

                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.Ok(new { received = true }) : result.ToErrorResult(statusCode: 400);
            })
            .WithName("StripeWebhook")
            .WithSummary("Stripe asynchronous webhook callback")
            .AllowAnonymous();

        // POST /api/payments/webhooks/paypal
        group.MapPost("/webhooks/paypal", async (
                HttpRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var rawBody = await ReadRawBodyAsync(request);
                var sigHeader = request.Headers["PAYPAL-TRANSMISSION-SIG"].ToString();
                var eventType = request.Headers["PAYPAL-EVENT-TYPE"].ToString();

                var command = new IngestWebhookCommand(
                    Provider: PaymentProvider.PayPal,
                    RawBody: rawBody,
                    SignatureHeader: sigHeader,
                    EventType: eventType,
                    TimestampHeader: request.Headers["PAYPAL-TRANSMISSION-TIME"].ToString()
                );

                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.Ok(new { received = true }) : result.ToErrorResult(statusCode: 400);
            })
            .WithName("PayPalWebhook")
            .WithSummary("PayPal asynchronous webhook callback")
            .AllowAnonymous();

        // POST /api/payments/webhooks/vnpay
        group.MapPost("/webhooks/vnpay", async (
                HttpRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var rawBody = request.QueryString.HasValue
                    ? request.QueryString.Value!.TrimStart('?')
                    : await ReadRawBodyAsync(request);

                var command = new IngestWebhookCommand(
                    Provider: PaymentProvider.VNPay,
                    RawBody: rawBody,
                    SignatureHeader: null,
                    EventType: null,
                    TimestampHeader: null
                );

                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.Ok(new { RspCode = "00", Message = "Confirm Success" }) : Results.Ok(new { RspCode = "97", Message = "Invalid Checksum" });
            })
            .WithName("VNPayWebhook")
            .WithSummary("VNPay IPN webhook callback")
            .AllowAnonymous();

        // POST /api/payments/webhooks/momo
        group.MapPost("/webhooks/momo", async (
                HttpRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var rawBody = await ReadRawBodyAsync(request);

                var command = new IngestWebhookCommand(
                    Provider: PaymentProvider.MoMo,
                    RawBody: rawBody,
                    SignatureHeader: null,
                    EventType: null,
                    TimestampHeader: null
                );

                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.NoContent() : result.ToErrorResult(statusCode: 400);
            })
            .WithName("MoMoWebhook")
            .WithSummary("MoMo IPN webhook callback")
            .AllowAnonymous();

        // POST /api/payments/sandbox/complete — Complete a sandbox test payment
        group.MapPost("/sandbox/complete", async (
                [FromBody] SandboxCompleteRequest request,
                [FromServices] IPaymentRepository paymentRepository,
                [FromServices] IBookingRepository bookingRepository,
                [FromServices] IMediator mediator,
                CancellationToken ct) =>
            {
                var payment = await paymentRepository.GetByBookingIdAsync(request.BookingId, ct);
                if (payment == null)
                    return Results.NotFound(new { Message = "Payment not found for booking." });

                payment.TransitionTo(
                    request.Success ? PaymentTransactionStatus.Succeeded : PaymentTransactionStatus.Failed,
                    DateTime.UtcNow,
                    "{\"simulated\": true, \"sandbox\": true}",
                    request.Success ? null : "Simulated payment failure"
                );
                await paymentRepository.UpdateAsync(payment, ct);

                if (request.Success)
                {
                    var booking = await bookingRepository.GetByIdAsync(payment.BookingId, ct);
                    if (booking != null)
                    {
                        booking.Status = "Confirmed";
                        await bookingRepository.UpdateAsync(booking, ct);
                        await mediator.Publish(new AirlineTicket.BuildingBlocks.Application.Events.BookingConfirmedEvent(booking.Id, "Sandbox"), ct);
                    }
                }

                return Results.Ok(new { Success = true, Status = payment.Status.ToString() });
            })
            .WithName("SandboxCompletePayment")
            .WithSummary("Complete a sandbox test payment")
            .Produces(200)
            .Produces(404)
            .AllowAnonymous();
    }

    private static async Task<string> ReadRawBodyAsync(HttpRequest request)
    {
        request.EnableBuffering();
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        return body;
    }
}

public sealed record CheckoutRequest(
    Guid BookingId,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] PaymentProvider Provider,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] Currency Currency,
    string ReturnUrl,
    string CancelUrl
);

public sealed record SandboxCompleteRequest(Guid BookingId, bool Success = true);
