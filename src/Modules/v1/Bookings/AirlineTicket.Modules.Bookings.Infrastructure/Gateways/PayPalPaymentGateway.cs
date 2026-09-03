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

public class PayPalPaymentGateway : IPaymentGateway
{
    private readonly HttpClient _httpClient;
    private readonly PayPalOptions _options;
    private readonly ILogger<PayPalPaymentGateway> _logger;

    public PaymentProvider Provider => PaymentProvider.PayPal;

    public PayPalPaymentGateway(
        HttpClient httpClient,
        IOptions<PaymentGatewayOptions> options,
        ILogger<PayPalPaymentGateway> logger)
    {
        _httpClient = httpClient;
        _options = options.Value.PayPal;
        _logger = logger;
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{_options.BaseUrl}/v1/oauth2/token");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
        request.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"PayPal auth failed: {content}");
        }

        using var doc = JsonDocument.Parse(content);
        return doc.RootElement.GetProperty("access_token").GetString()!;
    }

    public async Task<CreatePaymentResult> CreatePaymentUrlAsync(PaymentCheckoutRequest request, CancellationToken cancellationToken = default)
    {
        var token = await GetAccessTokenAsync(cancellationToken);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_options.BaseUrl}/v2/checkout/orders");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var currency = request.Money.Currency.ToString();
        var amountStr = request.Money.Amount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);

        var orderPayload = new
        {
            intent = "CAPTURE",
            purchase_units = new[]
            {
                new
                {
                    reference_id = request.BookingId,
                    description = request.Description,
                    amount = new
                    {
                        currency_code = currency,
                        value = amountStr
                    }
                }
            },
            application_context = new
            {
                return_url = request.ReturnUrl,
                cancel_url = request.CancelUrl,
                user_action = "PAY_NOW"
            }
        };

        httpRequest.Content = new StringContent(JsonSerializer.Serialize(orderPayload), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("PayPal Order creation failed: {Content}", content);
            throw new InvalidOperationException($"PayPal order failed: {response.StatusCode}");
        }

        using var doc = JsonDocument.Parse(content);
        var orderId = doc.RootElement.GetProperty("id").GetString()!;

        string? approveUrl = null;
        if (doc.RootElement.TryGetProperty("links", out var links))
        {
            foreach (var link in links.EnumerateArray())
            {
                if (link.GetProperty("rel").GetString() == "approve")
                {
                    approveUrl = link.GetProperty("href").GetString();
                    break;
                }
            }
        }

        return new CreatePaymentResult(
            ProviderTransactionId: orderId,
            Provider: Provider,
            PaymentUrl: approveUrl,
            ClientSecret: null,
            OrderId: orderId
        );
    }

    public Task<PaymentWebhookResult> ProcessWebhookAsync(WebhookPayload payload, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(payload.RawBody);
            var root = doc.RootElement;
            var eventId = root.GetProperty("id").GetString()!;
            var eventType = root.GetProperty("event_type").GetString()!;
            var resource = root.GetProperty("resource");

            var orderId = resource.TryGetProperty("id", out var idProp) ? idProp.GetString() : eventId;
            var status = eventType switch
            {
                "PAYMENT.CAPTURE.COMPLETED" or "CHECKOUT.ORDER.APPROVED" => PaymentTransactionStatus.Succeeded,
                "PAYMENT.CAPTURE.DENIED" => PaymentTransactionStatus.Failed,
                "CHECKOUT.ORDER.CANCELLED" => PaymentTransactionStatus.Cancelled,
                _ => PaymentTransactionStatus.Processing
            };

            return Task.FromResult(new PaymentWebhookResult(
                IsValid: true,
                ProviderEventId: eventId,
                ProviderTransactionId: orderId,
                Status: status,
                FailureReason: status == PaymentTransactionStatus.Failed ? "PayPal payment capture denied" : null,
                ProviderTimestamp: DateTime.UtcNow,
                RawJson: payload.RawBody
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PayPal webhook parse exception.");
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
        return Task.FromResult(new RefundResult(true, $"pp_ref_{Guid.NewGuid():N}", null));
    }

    public async Task<PaymentStatusResult> QueryTransactionStatusAsync(string providerTransactionId, CancellationToken cancellationToken = default)
    {
        var token = await GetAccessTokenAsync(cancellationToken);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{_options.BaseUrl}/v2/checkout/orders/{providerTransactionId}");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

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
            "COMPLETED" => PaymentTransactionStatus.Succeeded,
            "APPROVED" => PaymentTransactionStatus.Processing,
            "VOIDED" => PaymentTransactionStatus.Cancelled,
            "CREATED" or "SAVED" => PaymentTransactionStatus.Pending,
            _ => PaymentTransactionStatus.Failed
        };

        return new PaymentStatusResult(providerTransactionId, status, null, DateTime.UtcNow);
    }
}
