using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
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

public class MoMoPaymentGateway : IPaymentGateway
{
    private readonly HttpClient _httpClient;
    private readonly MoMoOptions _options;
    private readonly ILogger<MoMoPaymentGateway> _logger;

    public PaymentProvider Provider => PaymentProvider.MoMo;

    public MoMoPaymentGateway(
        HttpClient httpClient,
        IOptions<PaymentGatewayOptions> options,
        ILogger<MoMoPaymentGateway> logger)
    {
        _httpClient = httpClient;
        _options = options.Value.MoMo;
        _logger = logger;
    }

    public async Task<CreatePaymentResult> CreatePaymentUrlAsync(PaymentCheckoutRequest request, CancellationToken cancellationToken = default)
    {
        var orderId = $"{request.BookingId}_{DateTime.UtcNow.Ticks}";
        var requestId = Guid.NewGuid().ToString("N");
        var amount = (long)request.Money.Amount;
        var orderInfo = request.Description;
        var redirectUrl = request.ReturnUrl;
        var ipnUrl = request.ReturnUrl; // Overridden by controller webhook route in prod
        var extraData = "";
        var requestType = "captureWallet";

        var rawSignature = $"accessKey={_options.AccessKey}&amount={amount}&extraData={extraData}&ipnUrl={ipnUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={_options.PartnerCode}&redirectUrl={redirectUrl}&requestId={requestId}&requestType={requestType}";
        var signature = HmacSha256(_options.SecretKey, rawSignature);

        var requestBody = new
        {
            partnerCode = _options.PartnerCode,
            partnerName = "AirlineTicket",
            storeId = "AirlineTicketStore",
            requestId = requestId,
            amount = amount,
            orderId = orderId,
            orderInfo = orderInfo,
            redirectUrl = redirectUrl,
            ipnUrl = ipnUrl,
            lang = "vi",
            extraData = extraData,
            requestType = requestType,
            signature = signature
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, _options.Endpoint);
        httpRequest.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("MoMo Create Payment failed: {Response}", content);
            throw new InvalidOperationException($"MoMo API returned status {response.StatusCode}.");
        }

        using var doc = JsonDocument.Parse(content);
        var payUrl = doc.RootElement.GetProperty("payUrl").GetString();

        return new CreatePaymentResult(
            ProviderTransactionId: orderId,
            Provider: Provider,
            PaymentUrl: payUrl,
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

            var partnerCode = root.GetProperty("partnerCode").GetString();
            var orderId = root.GetProperty("orderId").GetString()!;
            var requestId = root.GetProperty("requestId").GetString();
            var amount = root.GetProperty("amount").GetInt64();
            var orderInfo = root.GetProperty("orderInfo").GetString();
            var orderType = root.GetProperty("orderType").GetString();
            var transId = root.GetProperty("transId").GetInt64().ToString();
            var resultCode = root.GetProperty("resultCode").GetInt32();
            var message = root.GetProperty("message").GetString();
            var payType = root.GetProperty("payType").GetString();
            var responseTime = root.GetProperty("responseTime").GetInt64();
            var extraData = root.GetProperty("extraData").GetString();
            var signature = root.GetProperty("signature").GetString();

            var rawSignature = $"accessKey={_options.AccessKey}&amount={amount}&extraData={extraData}&message={message}&orderId={orderId}&orderInfo={orderInfo}&orderType={orderType}&partnerCode={partnerCode}&payType={payType}&requestId={requestId}&responseTime={responseTime}&resultCode={resultCode}&transId={transId}";
            var computedSignature = HmacSha256(_options.SecretKey, rawSignature);
            var isValid = string.Equals(signature, computedSignature, StringComparison.OrdinalIgnoreCase);

            var status = (isValid && resultCode == 0)
                ? PaymentTransactionStatus.Succeeded
                : PaymentTransactionStatus.Failed;

            return Task.FromResult(new PaymentWebhookResult(
                IsValid: isValid,
                ProviderEventId: transId,
                ProviderTransactionId: orderId,
                Status: status,
                FailureReason: status == PaymentTransactionStatus.Failed ? message : null,
                ProviderTimestamp: DateTime.UtcNow,
                RawJson: payload.RawBody
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MoMo webhook processing failed.");
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
        return Task.FromResult(new RefundResult(true, $"momo_ref_{Guid.NewGuid():N}", null));
    }

    public Task<PaymentStatusResult> QueryTransactionStatusAsync(string providerTransactionId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new PaymentStatusResult(
            providerTransactionId,
            PaymentTransactionStatus.Succeeded,
            null,
            DateTime.UtcNow
        ));
    }

    private static string HmacSha256(string key, string data)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(dataBytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
