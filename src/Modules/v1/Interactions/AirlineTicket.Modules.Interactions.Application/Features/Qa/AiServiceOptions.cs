namespace AirlineTicket.Modules.Interactions.Application.Features.Qa;

/// <summary>
/// Cấu hình AI service (Model Store API Key, endpoint, model name, v.v.).
/// Ánh xạ từ section "AiService" trong appsettings.json / User Secrets.
/// </summary>
public sealed class AiServiceOptions
{
    public const string SectionName = "AiService";

    public ModelStoreOptions ModelStore { get; set; } = new();
}

/// <summary>
/// Cấu hình endpoint Model Store tương thích OpenAI (ví dụ NVIDIA Integrate, OpenAI, Azure OpenAI).
/// </summary>
public sealed class ModelStoreOptions
{
    public const string SectionName = "ModelStore";

    /// <summary>
    /// Base URL của endpoint tương thích OpenAI. Ví dụ: https://integrate.api.nvidia.com/v1
    /// </summary>
    public string BaseUrl { get; set; } = "https://integrate.api.nvidia.com/v1";

    /// <summary>
    /// API key để truy cập Model Store AI service.
    /// KHÔNG commit giá trị thật vào source control — dùng User Secrets / biến môi trường.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Tên model chat completion. Mặc định: deepseek-ai/deepseek-v4-flash.
    /// </summary>
    public string Model { get; set; } = "deepseek-ai/deepseek-v4-flash";

    /// <summary>Nhiệt độ sampling (0..2). Càng cao càng ngẫu nhiên.</summary>
    public double Temperature { get; set; } = 1.0;

    /// <summary>Nucleus sampling (0..1).</summary>
    public double TopP { get; set; } = 0.95;

    /// <summary>Số token tối đa trong câu trả lời.</summary>
    public int MaxTokens { get; set; } = 16384;

    /// <summary>Bật chế độ suy luận (chain-of-thought) của model nếu được hỗ trợ.</summary>
    public bool EnableThinking { get; set; } = true;

    /// <summary>Mức độ suy luận khi <see cref="EnableThinking"/> bật: low | medium | high.</summary>
    public string ReasoningEffort { get; set; } = "high";

    /// <summary>Thời gian chờ tối đa cho một request (giây).</summary>
    public int TimeoutSeconds { get; set; } = 100;
}
