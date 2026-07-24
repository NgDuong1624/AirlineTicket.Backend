using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;

namespace AirlineTicket.Modules.Interactions.Application.Features.Qa;

/// <summary>
/// Handler for processing flight ticket questions by calling Model Store AI (OpenAI-compatible endpoint).
/// Separates HTTP calls into <see cref="IAirTicketAiClient"/> (Infrastructure) to keep the Application layer pure.
/// </summary>
public sealed class GetAirTicketAnswerQueryHandler : IQueryHandler<GetAirTicketAnswerQuery, AirTicketAnswerResponse>
{
    private readonly IAirTicketAiClient _aiClient;

    public GetAirTicketAnswerQueryHandler(IAirTicketAiClient aiClient)
    {
        _aiClient = aiClient ?? throw new ArgumentNullException(nameof(aiClient));
    }

    public async Task<AirTicketAnswerResponse> Handle(GetAirTicketAnswerQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            throw new ArgumentException("Question cannot be empty.", nameof(request));
        }

        var result = await _aiClient.AskAsync(request.Question.Trim(), request.History, request.Currency, cancellationToken);

        return new AirTicketAnswerResponse(result.Answer, result.Reasoning, result.Model, "Completed");
    }
}
