using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.Modules.Interactions.Application.Features.Qa;
using AirlineTicket.Modules.Interactions.Infrastructure.Ai;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AirlineTicket.Modules.Interactions.Api.Endpoints;

public class QaEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/qa")
            .WithTags("AI Chat");

        group.MapPost("/ask", async (
                [FromBody] AskQuestionRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                if (string.IsNullOrWhiteSpace(request.Question))
                {
                    return Results.BadRequest(new { Error = "Question is required." });
                }

                try
                {
                    var query = new GetAirTicketAnswerQuery(request.Question, request.History, request.Currency);
                    var response = await sender.Send(query, ct);
                    return Results.Ok(response);
                }
                catch (AiQuotaExceededException ex)
                {
                    return Results.Json(
                        new { Error = "AI service quota exceeded", Details = ex.Message },
                        statusCode: 429);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.Json(
                        new { Error = "AI service unavailable", Details = ex.Message },
                        statusCode: 503);
                }
                catch (Exception ex)
                {
                    return Results.Problem(
                        detail: ex.Message,
                        statusCode: 500,
                        title: "An unexpected error occurred");
                }
            })
            .WithName("AskQuestion")
            .WithSummary("Gửi câu hỏi cho AI Travel Assistant")
            .Produces<AirTicketAnswerResponse>(200)
            .Produces(400)
            .Produces(429)
            .Produces(503)
            .AllowAnonymous();
    }
}

public record AskQuestionRequest(string Question, List<ChatMessageDto>? History = null, string? Currency = null);
