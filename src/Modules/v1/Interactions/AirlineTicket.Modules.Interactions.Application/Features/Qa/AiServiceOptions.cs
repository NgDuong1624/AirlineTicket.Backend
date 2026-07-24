namespace AirlineTicket.Modules.Interactions.Application.Features.Qa;

/// <summary>
/// AI service configuration (Model Store API Key, endpoint, model name, etc.).
/// Mapped from "AiService" section in appsettings.json / User Secrets.
/// </summary>
public sealed class AiServiceOptions
{
    public const string SectionName = "AiService";

    public ModelStoreOptions ModelStore { get; set; } = new();
}

/// <summary>
/// Model Store endpoint configuration compatible with OpenAI (e.g., NVIDIA NIM, OpenAI, Azure OpenAI).
/// </summary>
public sealed class ModelStoreOptions
{
    public const string SectionName = "ModelStore";

    /// <summary>
    /// Base URL of the OpenAI-compatible endpoint. Example: https://api.9router.com/v1
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.9router.com/v1";

    /// <summary>
    /// API key to access the Model Store AI service.
    /// DO NOT commit the real value to source control — use User Secrets / environment variables.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Chat completion model name. Default: deepseek-ai/deepseek-v4-flash.
    /// </summary>
    public string Model { get; set; } = "deepseek-ai/deepseek-v4-flash";

    /// <summary>Sampling temperature (0..2). Higher is more random.</summary>
    public double Temperature { get; set; } = 1.0;

    /// <summary>Nucleus sampling (0..1).</summary>
    public double TopP { get; set; } = 0.95;

    /// <summary>Maximum number of tokens in the response.</summary>
    public int MaxTokens { get; set; } = 16384;

    /// <summary>Enable model's reasoning mode (chain-of-thought) if supported.</summary>
    public bool EnableThinking { get; set; } = true;

    /// <summary>Reasoning effort when <see cref="EnableThinking"/> is enabled: low | medium | high.</summary>
    public string ReasoningEffort { get; set; } = "high";

    /// <summary>Maximum timeout for a single request (seconds).</summary>
    public int TimeoutSeconds { get; set; } = 100;
}
