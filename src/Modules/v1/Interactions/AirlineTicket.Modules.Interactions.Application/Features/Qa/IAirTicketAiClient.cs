using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Interactions.Application.Features.Qa;

/// <summary>
/// Abstracts the call to Model Store AI (OpenAI-compatible endpoint).
/// Defined in the Application layer; concrete implementation (HttpClient) resides in the Infrastructure layer.
/// </summary>
public interface IAirTicketAiClient
{
    /// <summary>
    /// Sends the user's question to the model and returns the answer (along with reasoning if available).
    /// </summary>
    Task<AirTicketAiResult> AskAsync(
        string question,
        IReadOnlyList<ChatMessageDto>? history = null,
        string? currency = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a message in the conversation history.
/// </summary>
/// <param name="Role">Role (user, assistant).</param>
/// <param name="Content">Message content.</param>
public sealed record ChatMessageDto(string Role, string Content);

/// <summary>
/// Raw result returned from the AI client.
/// </summary>
/// <param name="Answer">Answer content for the user.</param>
/// <param name="Reasoning">Model's reasoning string (can be null if the model does not return it).</param>
/// <param name="Model">Name of the model that served the request.</param>
public sealed record AirTicketAiResult(string Answer, string? Reasoning, string Model);
