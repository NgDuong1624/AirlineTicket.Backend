using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Interactions.Application.Features.Qa;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AirlineTicket.Modules.Interactions.Infrastructure.Ai;

/// <summary>
/// Cài đặt <see cref="IAirTicketAiClient"/> gọi tới endpoint Chat Completions tương thích OpenAI
/// (mặc định: NVIDIA Integrate API + DeepSeek). Đăng ký dưới dạng typed <see cref="HttpClient"/>.
/// </summary>
public sealed class NvidiaModelStoreQaClient : IAirTicketAiClient
{
    private const string SystemPrompt =
        "You are the virtual customer-service assistant for an airline ticketing platform. " +
        "Answer questions about bookings, cancellations, refunds, baggage, check-in, flight changes " +
        "and fares clearly and concisely. If a question is unrelated to air travel or you are unsure, " +
        "say so and suggest contacting human support. Never invent fares, dates or policy numbers.";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly ModelStoreOptions _options;
    private readonly ILogger<NvidiaModelStoreQaClient> _logger;

    public NvidiaModelStoreQaClient(
        HttpClient httpClient,
        IOptions<AiServiceOptions> options,
        ILogger<NvidiaModelStoreQaClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = (options ?? throw new ArgumentNullException(nameof(options))).Value.ModelStore;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AirTicketAiResult> AskAsync(string question, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException(
                "AiService:ModelStore:ApiKey is not configured. Set it via User Secrets or environment variables.");
        }

        var payload = new ChatCompletionRequest
        {
            Model = _options.Model,
            Temperature = _options.Temperature,
            TopP = _options.TopP,
            MaxTokens = _options.MaxTokens,
            Stream = false,
            Messages = new[]
            {
                new ChatMessage("system", SystemPrompt),
                new ChatMessage("user", question)
            },
            ChatTemplateKwargs = _options.EnableThinking
                ? new ChatTemplateKwargs { Thinking = true, ReasoningEffort = _options.ReasoningEffort }
                : null
        };

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsJsonAsync("chat/completions", payload, SerializerOptions, cancellationToken);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "AI request timed out after {Timeout}s.", _options.TimeoutSeconds);
            throw new InvalidOperationException("The AI service did not respond in time. Please try again.", ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to reach the AI service at {BaseUrl}.", _options.BaseUrl);
            throw new InvalidOperationException("Unable to reach the AI service. Please try again later.", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("AI service returned {StatusCode}: {Body}", (int)response.StatusCode, body);
            throw new InvalidOperationException($"AI service returned an error ({(int)response.StatusCode}).");
        }

        var completion = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(SerializerOptions, cancellationToken);

        var message = completion?.Choices is { Length: > 0 } choices ? choices[0].Message : null;
        if (message is null || string.IsNullOrWhiteSpace(message.Content))
        {
            _logger.LogWarning("AI service returned an empty completion.");
            throw new InvalidOperationException("The AI service returned an empty response.");
        }

        var reasoning = message.Reasoning ?? message.ReasoningContent;
        return new AirTicketAiResult(message.Content.Trim(), reasoning, completion!.Model ?? _options.Model);
    }

    // ---- Request DTOs (OpenAI-compatible Chat Completions) ----

    private sealed class ChatCompletionRequest
    {
        [JsonPropertyName("model")] public required string Model { get; init; }
        [JsonPropertyName("messages")] public required IReadOnlyList<ChatMessage> Messages { get; init; }
        [JsonPropertyName("temperature")] public double Temperature { get; init; }
        [JsonPropertyName("top_p")] public double TopP { get; init; }
        [JsonPropertyName("max_tokens")] public int MaxTokens { get; init; }
        [JsonPropertyName("stream")] public bool Stream { get; init; }

        /// <summary>Tương đương extra_body.chat_template_kwargs của OpenAI Python SDK.</summary>
        [JsonPropertyName("chat_template_kwargs")] public ChatTemplateKwargs? ChatTemplateKwargs { get; init; }
    }

    private sealed record ChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private sealed class ChatTemplateKwargs
    {
        [JsonPropertyName("thinking")] public bool Thinking { get; init; }
        [JsonPropertyName("reasoning_effort")] public string ReasoningEffort { get; init; } = "high";
    }

    // ---- Response DTOs ----

    private sealed class ChatCompletionResponse
    {
        [JsonPropertyName("model")] public string? Model { get; init; }
        [JsonPropertyName("choices")] public ChatChoice[]? Choices { get; init; }
    }

    private sealed class ChatChoice
    {
        [JsonPropertyName("message")] public ResponseMessage? Message { get; init; }
    }

    private sealed class ResponseMessage
    {
        [JsonPropertyName("content")] public string? Content { get; init; }
        [JsonPropertyName("reasoning")] public string? Reasoning { get; init; }
        [JsonPropertyName("reasoning_content")] public string? ReasoningContent { get; init; }
    }
}
