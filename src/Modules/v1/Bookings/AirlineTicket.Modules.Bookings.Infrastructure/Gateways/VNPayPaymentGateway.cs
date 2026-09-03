using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Domain.Enums;
using AirlineTicket.Modules.Bookings.Infrastructure.Gateways.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AirlineTicket.Modules.Bookings.Infrastructure.Gateways;

public class VNPayPaymentGateway : IPaymentGateway
{
    private readonly HttpClient _httpClient;
    private readonly VNPayOptions _options;
    private readonly ILogger<VNPayPaymentGateway> _logger;

    public PaymentProvider Provider => PaymentProvider.VNPay;

    public VNPayPaymentGateway(
        HttpClient httpClient,
        IOptions<PaymentGatewayOptions> options,
        ILogger<VNPayPaymentGateway> logger)
    {
        _httpClient = httpClient;
        _options = options.Value.VNPay;
        _logger = logger;
    }

    public Task<CreatePaymentResult> CreatePaymentUrlAsync(PaymentCheckoutRequest request, CancellationToken cancellationToken = default)
    {
        var txnRef = $"{request.BookingId}_{DateTime.UtcNow.Ticks}";
        var amountInVnd = (long)request.Money.Amount * 100; // VNPay multiplies by 100

        var vnpParams = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            { "vnp_Version", _options.Version },
            { "vnp_Command", _options.Command },
            { "vnp_TmnCode", _options.TmnCode },
            { "vnp_Amount", amountInVnd.ToString() },
            { "vnp_CreateDate", DateTime.UtcNow.ToString("yyyyMMddHHmmss") },
            { "vnp_CurrCode", "VND" },
            { "vnp_IpAddr", "127.0.0.1" },
            { "vnp_Locale", "vn" },
            { "vnp_OrderInfo", request.Description },
            { "vnp_OrderType", "other" },
            { "vnp_ReturnUrl", request.ReturnUrl },
            { "vnp_TxnRef", txnRef }
        };

        var hashData = new StringBuilder();
        var query = new StringBuilder();

        foreach (var (key, value) in vnpParams)
        {
            if (!string.IsNullOrEmpty(value))
            {
                hashData.Append(WebUtility.UrlEncode(key)).Append('=').Append(WebUtility.UrlEncode(value)).Append('&');
                query.Append(WebUtility.UrlEncode(key)).Append('=').Append(WebUtility.UrlEncode(value)).Append('&');
            }
        }

        if (hashData.Length > 0)
        {
            hashData.Remove(hashData.Length - 1, 1);
            query.Remove(query.Length - 1, 1);
        }

        var secureHash = HmacSha512(_options.HashSecret, hashData.ToString());
        var paymentUrl = $"{_options.BaseUrl}?{query}&vnp_SecureHash={secureHash}";

        return Task.FromResult(new CreatePaymentResult(
            ProviderTransactionId: txnRef,
            Provider: Provider,
            PaymentUrl: paymentUrl,
            ClientSecret: null,
            OrderId: txnRef
        ));
    }

    public Task<PaymentWebhookResult> ProcessWebhookAsync(WebhookPayload payload, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = HttpUtility.ParseQueryString(payload.RawBody);
            var vnpParams = new SortedDictionary<string, string>(StringComparer.Ordinal);
            string? inputHash = null;

            foreach (string? key in query.AllKeys)
            {
                if (string.IsNullOrEmpty(key)) continue;
                var val = query[key];
                if (key.Equals("vnp_SecureHash", StringComparison.OrdinalIgnoreCase))
                {
                    inputHash = val;
                }
                else if (key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(val))
                {
                    vnpParams.Add(key, val);
                }
            }

            var hashData = new StringBuilder();
            foreach (var (k, v) in vnpParams)
            {
                hashData.Append(WebUtility.UrlEncode(k)).Append('=').Append(WebUtility.UrlEncode(v)).Append('&');
            }
            if (hashData.Length > 0) hashData.Remove(hashData.Length - 1, 1);

            var calculatedHash = HmacSha512(_options.HashSecret, hashData.ToString());
            var isValid = string.Equals(calculatedHash, inputHash, StringComparison.OrdinalIgnoreCase);

            var responseCode = query["vnp_ResponseCode"];
            var transactionStatus = query["vnp_TransactionStatus"];
            var txnRef = query["vnp_TxnRef"] ?? string.Empty;
            var transactionNo = query["vnp_TransactionNo"] ?? txnRef;

            var status = (isValid && responseCode == "00" && transactionStatus == "00")
                ? PaymentTransactionStatus.Succeeded
                : PaymentTransactionStatus.Failed;

            return Task.FromResult(new PaymentWebhookResult(
                IsValid: isValid,
                ProviderEventId: transactionNo,
                ProviderTransactionId: txnRef,
                Status: status,
                FailureReason: status == PaymentTransactionStatus.Failed ? $"VNPay Error Code: {responseCode}" : null,
                ProviderTimestamp: DateTime.UtcNow,
                RawJson: payload.RawBody
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "VNPay webhook processing failed.");
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
        return Task.FromResult(new RefundResult(true, $"vnp_ref_{Guid.NewGuid():N}", null));
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

    private static string HmacSha512(string key, string inputData)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var inputBytes = Encoding.UTF8.GetBytes(inputData);
        using var hmac = new HMACSHA512(keyBytes);
        var hashValue = hmac.ComputeHash(inputBytes);
        return Convert.ToHexString(hashValue).ToLowerInvariant();
    }
}
