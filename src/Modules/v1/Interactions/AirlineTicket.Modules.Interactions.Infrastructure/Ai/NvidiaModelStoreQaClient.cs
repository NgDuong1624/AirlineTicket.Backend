using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.Application.Contracts;
using AirlineTicket.Modules.Interactions.Application.Features.Qa;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AirlineTicket.Modules.Interactions.Infrastructure.Ai;

/// <summary>
/// Cài đặt <see cref="IAirTicketAiClient"/> gọi tới endpoint Chat Completions tương thích OpenAI.
/// Hỗ trợ Tool Calling (Function Calling) để lấy dữ liệu thực tế từ module Flights.
/// </summary>
public sealed class NvidiaModelStoreQaClient : IAirTicketAiClient
{
    private const string SystemPrompt =
        "# Role & Objective\n" +
        "You are an expert AI Travel Assistant for an online flight booking platform. " +
        "Your goal is to help users find flights, provide accurate schedule and pricing information, " +
        "and guide them directly to the booking page using real-time data.\n\n" +
        "# Core Rules & Constraints\n" +
        "1. NO HALLUCINATION: You do NOT know the flight schedules, seat availability, or prices on your own. " +
        "You MUST ALWAYS use the `search_flights` tool to fetch real-time data when a user asks about flight availability. Never invent flight numbers, times, or prices.\n" +
        "2. CURRENT DATE CONTEXT: Today is Sunday, June 21, 2026. Use this to calculate relative dates (e.g., 'tomorrow' is June 22, 2026, 'next Monday' is June 22, 2026, etc.).\n" +
        "3. MANDATORY BOOKING LINKS: When presenting flight options, you MUST explicitly include the exact markdown booking URL (e.g., `[Book Now](https://...)`) provided in the tool's data source.\n" +
        "4. TONALITY: Professional, helpful, enthusiastic, and concise. Respond in the **same language as the user's question**. If the user asks in English, answer in English. If they ask in Vietnamese, answer in Vietnamese. Never default to Vietnamese — always match the questioner's language.\n\n" +
        "# Workflow\n" +
        "- Step 1: Analyze the user's request to extract departure, destination, and travel date. (If any info is missing, politely ask the user to clarify).\n" +
        "- Step 2: Trigger the `search_flights` tool with the extracted parameters.\n" +
        "- Step 3: Read the JSON data returned by the tool.\n" +
        "- Step 4: Synthesize the flight options into a clean, easy-to-read list (formatted in Markdown) for the user, ensuring the clickable booking links are naturally integrated.";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly ModelStoreOptions _options;
    private readonly ISharedFlightSearchService _flightSearchService;
    private readonly ILogger<NvidiaModelStoreQaClient> _logger;

    public NvidiaModelStoreQaClient(
        HttpClient httpClient,
        IOptions<AiServiceOptions> options,
        ISharedFlightSearchService flightSearchService,
        ILogger<NvidiaModelStoreQaClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = (options ?? throw new ArgumentNullException(nameof(options))).Value.ModelStore;
        _flightSearchService = flightSearchService ?? throw new ArgumentNullException(nameof(flightSearchService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AirTicketAiResult> AskAsync(string question, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("AiService:ModelStore:ApiKey is not configured.");
        }

        var messages = new List<ChatMessage>
        {
            new ChatMessage { Role = "system", Content = SystemPrompt },
            new ChatMessage { Role = "user", Content = question }
        };

        var requestPayload = CreateRequest(messages);
        var response = await SendRequestAsync(requestPayload, cancellationToken);
        var completion = await ParseResponseAsync(response, cancellationToken);

        var message = completion?.Choices is { Length: > 0 } choices ? choices[0].Message : null;
        if (message is null)
        {
            throw new InvalidOperationException("The AI service returned an empty response.");
        }

        // Loop for tool calls (we only do one depth of loop to prevent infinite recursive bugs)
        if (message.ToolCalls != null && message.ToolCalls.Length > 0)
        {
            _logger.LogInformation("AI decided to call tools: {ToolCount} tool(s).", message.ToolCalls.Length);

            // Append assistant message (with tool_calls) to history
            messages.Add(new ChatMessage
            {
                Role = "assistant",
                Content = message.Content,
                ToolCalls = message.ToolCalls
            });

            // Execute each tool and append tool results
            foreach (var toolCall in message.ToolCalls)
            {
                if (toolCall.Function != null && toolCall.Function.Name == "search_flights")
                {
                    string toolResultJson;
                    try
                    {
                        var args = JsonSerializer.Deserialize<SearchFlightsArgs>(toolCall.Function.Arguments, SerializerOptions);
                        if (args != null && !string.IsNullOrEmpty(args.OriginCode) && !string.IsNullOrEmpty(args.DestinationCode))
                        {
                            if (!DateTime.TryParse(args.Date, out var date))
                            {
                                date = new DateTime(2026, 6, 21); // Fallback to current date context
                            }

                            _logger.LogInformation("Executing tool search_flights for {Origin} -> {Dest} on {Date}",
                                args.OriginCode, args.DestinationCode, date.ToString("yyyy-MM-dd"));

                            toolResultJson = await _flightSearchService.SearchFlightsJsonAsync(
                                args.OriginCode, args.DestinationCode, date, args.CabinClass, cancellationToken);
                        }
                        else
                        {
                            toolResultJson = "{\"error\": \"Missing required parameters originCode and destinationCode.\"}";
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error executing search_flights tool.");
                        toolResultJson = $"{{\"error\": \"Tool execution failed: {ex.Message}\"}}";
                    }

                    messages.Add(new ChatMessage
                    {
                        Role = "tool",
                        Content = toolResultJson,
                        ToolCallId = toolCall.Id
                    });
                }
            }

            // Second round-trip with tool results
            var secondRequestPayload = CreateRequest(messages);
            var secondResponse = await SendRequestAsync(secondRequestPayload, cancellationToken);
            var secondCompletion = await ParseResponseAsync(secondResponse, cancellationToken);

            var secondMessage = secondCompletion?.Choices is { Length: > 0 } choices2 ? choices2[0].Message : null;
            if (secondMessage is null || string.IsNullOrWhiteSpace(secondMessage.Content))
            {
                throw new InvalidOperationException("The AI service returned an empty response after tool execution.");
            }

            var reasoning2 = secondMessage.Reasoning ?? secondMessage.ReasoningContent;
            return new AirTicketAiResult(secondMessage.Content.Trim(), reasoning2, secondCompletion!.Model ?? _options.Model);
        }

        // Direct response without tools
        if (string.IsNullOrWhiteSpace(message.Content))
        {
            throw new InvalidOperationException("The AI service returned an empty response.");
        }

        var reasoning = message.Reasoning ?? message.ReasoningContent;
        return new AirTicketAiResult(message.Content.Trim(), reasoning, completion!.Model ?? _options.Model);
    }

    private ChatCompletionRequest CreateRequest(List<ChatMessage> messages)
    {
        return new ChatCompletionRequest
        {
            Model = _options.Model,
            Temperature = _options.Temperature,
            TopP = _options.TopP,
            MaxTokens = _options.MaxTokens,
            Stream = false,
            Messages = messages,
            ChatTemplateKwargs = _options.EnableThinking
                ? new ChatTemplateKwargs { Thinking = true, ReasoningEffort = _options.ReasoningEffort }
                : null,
            Tools = new[]
            {
                new ToolDefinition
                {
                    Type = "function",
                    Function = new FunctionDefinition
                    {
                        Name = "search_flights",
                        Description = "Searches for available flights between origin and destination on a specific date.",
                        Parameters = new
                        {
                            type = "object",
                            properties = new Dictionary<string, object>
                            {
                                { "originCode", new { type = "string", description = "The 3-letter IATA code of the departure airport (e.g. HAN)." } },
                                { "destinationCode", new { type = "string", description = "The 3-letter IATA code of the arrival airport (e.g. SGN)." } },
                                { "date", new { type = "string", description = "The departure travel date in YYYY-MM-DD format." } },
                                { "cabinClass", new { type = "string", description = "Optional cabin class, e.g. Economy, Business." } }
                            },
                            required = new[] { "originCode", "destinationCode", "date" }
                        }
                    }
                }
            }
        };
    }

    private async Task<HttpResponseMessage> SendRequestAsync(ChatCompletionRequest payload, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("chat/completions", payload, SerializerOptions, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("AI service returned {StatusCode}: {Body}", (int)response.StatusCode, body);
                throw new InvalidOperationException($"AI service returned an error ({(int)response.StatusCode}).");
            }
            return response;
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
    }

    private async Task<ChatCompletionResponse?> ParseResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        return await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(SerializerOptions, cancellationToken);
    }

    // ---- Request DTOs (OpenAI-compatible Chat Completions) ----

    private sealed class SearchFlightsArgs
    {
        [JsonPropertyName("originCode")] public string? OriginCode { get; set; }
        [JsonPropertyName("destinationCode")] public string? DestinationCode { get; set; }
        [JsonPropertyName("date")] public string? Date { get; set; }
        [JsonPropertyName("cabinClass")] public string? CabinClass { get; set; }
    }

    private sealed class ChatCompletionRequest
    {
        [JsonPropertyName("model")] public required string Model { get; init; }
        [JsonPropertyName("messages")] public required IReadOnlyList<ChatMessage> Messages { get; init; }
        [JsonPropertyName("temperature")] public double Temperature { get; init; }
        [JsonPropertyName("top_p")] public double TopP { get; init; }
        [JsonPropertyName("max_tokens")] public int MaxTokens { get; init; }
        [JsonPropertyName("stream")] public bool Stream { get; init; }

        [JsonPropertyName("tools")] public ToolDefinition[]? Tools { get; init; }

        /// <summary>Tương đương extra_body.chat_template_kwargs của OpenAI Python SDK.</summary>
        [JsonPropertyName("chat_template_kwargs")] public ChatTemplateKwargs? ChatTemplateKwargs { get; init; }
    }

    private sealed class ChatMessage
    {
        [JsonPropertyName("role")] public required string Role { get; init; }
        [JsonPropertyName("content")] public string? Content { get; init; }

        [JsonPropertyName("tool_calls")] public ToolCall[]? ToolCalls { get; init; }
        [JsonPropertyName("tool_call_id")] public string? ToolCallId { get; init; }
    }

    private sealed class ToolDefinition
    {
        [JsonPropertyName("type")] public string Type { get; init; } = "function";
        [JsonPropertyName("function")] public required FunctionDefinition Function { get; init; }
    }

    private sealed class FunctionDefinition
    {
        [JsonPropertyName("name")] public required string Name { get; init; }
        [JsonPropertyName("description")] public required string Description { get; init; }
        [JsonPropertyName("parameters")] public required object Parameters { get; init; }
    }

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

        [JsonPropertyName("tool_calls")] public ToolCall[]? ToolCalls { get; init; }
    }

    private sealed class ToolCall
    {
        [JsonPropertyName("id")] public required string Id { get; init; }
        [JsonPropertyName("type")] public string Type { get; init; } = "function";
        [JsonPropertyName("function")] public required ToolCallFunction Function { get; init; }
    }

    private sealed class ToolCallFunction
    {
        [JsonPropertyName("name")] public required string Name { get; init; }
        [JsonPropertyName("arguments")] public required string Arguments { get; init; } // JSON string
    }
}
