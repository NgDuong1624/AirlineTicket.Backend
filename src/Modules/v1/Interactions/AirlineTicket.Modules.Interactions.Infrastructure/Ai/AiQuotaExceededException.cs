namespace AirlineTicket.Modules.Interactions.Infrastructure.Ai;

/// <summary>
/// Thrown when the AI service returns a quota-exceeded / rate-limit error.
/// Not transient — Polly should NOT retry.
/// </summary>
public sealed class AiQuotaExceededException : InvalidOperationException
{
    public AiQuotaExceededException(string message) : base(message)
    {
    }
}
