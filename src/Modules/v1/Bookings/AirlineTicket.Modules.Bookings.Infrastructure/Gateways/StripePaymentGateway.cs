using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Domain.Enums;
using AirlineTicket.Modules.Bookings.Infrastructure.Gateways.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AirlineTicket.Modules.Bookings.Infrastructure.Gateways;

public class StripePaymentGateway : IPaymentGateway
{
    private readonly HttpClient _httpClient;
    private readonly StripeOptions _options;
    private readonly ILogger<StripePaymentGateway> _logger;

    public PaymentProvider Provider => PaymentProvider.Stripe;

    public StripePaymentGateway(
        HttpClient httpClient,
        IOptions<PaymentGatewayOptions> options,
        ILogger<StripePaymentGateway> logger)
    {
        _httpClient = httpClient;
        _options = options.Value.Stripe;
        _logger = logger;
    }

    public async Task<CreatePaymentResult> CreatePaymentUrlAsync(PaymentCheckoutRequest request, CancellationToken cancellationToken = default)
    {
        var url = "https://api.stripe.com/v1/payment_intents";
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.SecretKey);

        var amountInCents = request.Money.ToGatewayUnits();
        var currencyCode = request.Money.Currency.ToString().ToLowerInvariant();

        var postParams = new Dictionary<string, string>
        {
            { "amount", amountInCents.ToString() },
            { "currency", currencyCode },
            { "description", request.Description },
            { "metadata[booking_id]", request.BookingId },
            { "receipt_email", request.CustomerEmail },
            { "automatic_payment_methods[enabled]", "true" }
        };

        httpRequest.Content = new FormUrlEncodedContent(postParams);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Stripe PaymentIntent creation failed: {Response}", responseContent);
            throw new InvalidOperationException($"Stripe PaymentIntent failed with status {response.StatusCode}.");
        }

        using var doc = JsonDocument.Parse(responseContent);
        var root = doc.RootElement;
        var id = root.GetProperty("id").GetString()!;
        var clientSecret = root.GetProperty("client_secret").GetString()!;

        return new CreatePaymentResult(
            ProviderTransactionId: id,
            Provider: Provider,
            PaymentUrl: null,
            ClientSecret: clientSecret,
            OrderId: id
        );
    }

    public Task<PaymentWebhookResult> ProcessWebhookAsync(WebhookPayload payload, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(payload.RawBody);
            var root = doc.RootElement;
            var eventId = root.GetProperty("id").GetString()!;
            var type = root.GetProperty("type").GetString()!;
            var dataObj = root.GetProperty("data").GetProperty("object");

            var transactionId = dataObj.TryGetProperty("id", out var idProp) ? idProp.GetString() : eventId;
            var status = type switch
            {
                "payment_intent.succeeded" => PaymentTransactionStatus.Succeeded,
                "payment_intent.payment_failed" => PaymentTransactionStatus.Failed,
                "payment_intent.canceled" => PaymentTransactionStatus.Cancelled,
                "payment_intent.processing" => PaymentTransactionStatus.Processing,
                _ => PaymentTransactionStatus.Pending
            };

            return Task.FromResult(new PaymentWebhookResult(
                IsValid: true,
                ProviderEventId: eventId,
                ProviderTransactionId: transactionId,
                Status: status,
                FailureReason: status == PaymentTransactionStatus.Failed ? "Payment intent failed" : null,
                ProviderTimestamp: DateTime.UtcNow,
                RawJson: payload.RawBody
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stripe webhook parsing error.");
            return Task.FromResult(new PaymentWebhookResult(
                IsValid: false,
                ProviderEventId: null,
                ProviderTransactionId: null,
                Status: PaymentTransactionStatus.Failed,
                FailureReason: ex.Message,
                ProviderTimestamp: null,
                RawJson: payload.RawBody
            ));
        }
    }

    public Task<RefundResult> RefundPaymentAsync(RefundRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new RefundResult(true, $"ref_{Guid.NewGuid():N}", null));
    }

    public async Task<PaymentStatusResult> QueryTransactionStatusAsync(string providerTransactionId, CancellationToken cancellationToken = default)
    {
        var url = $"https://api.stripe.com/v1/payment_intents/{providerTransactionId}";
        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.SecretKey);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return new PaymentStatusResult(providerTransactionId, PaymentTransactionStatus.Failed, content, DateTime.UtcNow);
        }

        using var doc = JsonDocument.Parse(content);
        var statusStr = doc.RootElement.GetProperty("status").GetString();

        var status = statusStr switch
        {
            "succeeded" => PaymentTransactionStatus.Succeeded,
            "processing" => PaymentTransactionStatus.Processing,
            "canceled" => PaymentTransactionStatus.Cancelled,
            "requires_payment_method" or "requires_confirmation" or "requires_action" => PaymentTransactionStatus.Pending,
            _ => PaymentTransactionStatus.Failed
        };

        return new PaymentStatusResult(providerTransactionId, status, null, DateTime.UtcNow);
    }
}
