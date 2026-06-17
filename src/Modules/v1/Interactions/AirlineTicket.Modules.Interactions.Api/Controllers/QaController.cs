using System;
using System.Threading.Tasks;
using AirlineTicket.Modules.Interactions.Application.Features.Qa;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AirlineTicket.Modules.Interactions.Api.Controllers;

[ApiController]
[Route("api/v1/qa")]
public class QaController : ControllerBase
{
    private readonly ISender _sender;

    public QaController(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    /// <summary>
    /// Gửi câu hỏi cho hệ thống AI của hãng hàng không.
    /// </summary>
    [HttpPost("ask")]
    public async Task<IActionResult> AskQuestion([FromBody] AskQuestionRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest(new { Error = "Question is required." });
        }

        try
        {
            var query = new GetAirTicketAnswerQuery(request.Question);
            var response = await _sender.Send(query);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            // Lỗi cấu hình thiếu hoặc AI service không khả dụng (timeout, lỗi upstream).
            return StatusCode(503, new { Error = "AI service unavailable", Details = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "An unexpected error occurred", Details = ex.Message });
        }
    }
}

public record AskQuestionRequest(string Question);