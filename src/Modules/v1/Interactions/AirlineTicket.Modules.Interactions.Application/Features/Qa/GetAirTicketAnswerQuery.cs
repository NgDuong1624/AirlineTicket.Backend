using System.Collections.Generic;
using AirlineTicket.BuildingBlocks.CQRS;

namespace AirlineTicket.Modules.Interactions.Application.Features.Qa;

/// <summary>
/// Query yêu cầu trả lời câu hỏi liên quan đến vé máy bay.
/// </summary>
public record GetAirTicketAnswerQuery(string Question, IReadOnlyList<ChatMessageDto>? History = null) : IQuery<AirTicketAnswerResponse>;

/// <summary>
/// Response chứa câu trả lời và siêu dữ liệu đi kèm.
/// </summary>
/// <param name="Answer">Câu trả lời hiển thị cho người dùng.</param>
/// <param name="Reasoning">Chuỗi suy luận của model (nếu có).</param>
/// <param name="Model">Tên model đã phục vụ request.</param>
/// <param name="Status">Trạng thái xử lý.</param>
public record AirTicketAnswerResponse(string Answer, string? Reasoning, string Model, string Status);
