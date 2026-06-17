using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Interactions.Application.Features.Qa;

/// <summary>
/// Trừu tượng hóa lời gọi tới Model Store AI (endpoint tương thích OpenAI).
/// Định nghĩa ở tầng Application; cài đặt cụ thể (HttpClient) nằm ở tầng Infrastructure.
/// </summary>
public interface IAirTicketAiClient
{
    /// <summary>
    /// Gửi câu hỏi của khách tới model và trả về câu trả lời (kèm phần suy luận nếu có).
    /// </summary>
    Task<AirTicketAiResult> AskAsync(string question, CancellationToken cancellationToken = default);
}

/// <summary>
/// Kết quả thô trả về từ AI client.
/// </summary>
/// <param name="Answer">Nội dung câu trả lời cho người dùng.</param>
/// <param name="Reasoning">Chuỗi suy luận của model (có thể null nếu model không trả về).</param>
/// <param name="Model">Tên model đã phục vụ request.</param>
public sealed record AirTicketAiResult(string Answer, string? Reasoning, string Model);
