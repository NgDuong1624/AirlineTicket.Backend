using System.Collections.Generic;
using AirlineTicket.BuildingBlocks.CQRS;

namespace AirlineTicket.Modules.Interactions.Application.Features.Qa;

/// <summary>
/// Query requesting an answer to a question related to flight tickets.
/// </summary>
public record GetAirTicketAnswerQuery(string Question, IReadOnlyList<ChatMessageDto>? History = null, string? Currency = null) : IQuery<AirTicketAnswerResponse>;

/// <summary>
/// Response containing the answer and accompanying metadata.
/// </summary>
/// <param name="Answer">Answer displayed to the user.</param>
/// <param name="Reasoning">Model's reasoning string (if any).</param>
/// <param name="Model">Name of the model that served the request.</param>
/// <param name="Status">Processing status.</param>
public record AirTicketAnswerResponse(string Answer, string? Reasoning, string Model, string Status);
