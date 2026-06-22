namespace AirlineTicket.BuildingBlocks.Logging;

/// <summary>
/// Structured log entry with correlation support for distributed tracing.
/// </summary>
public class LogEntry
{
    public string Message { get; init; } = string.Empty;
    public string Level { get; init; } = "Information";
    public string CorrelationId { get; init; } = string.Empty;
    public string? UserId { get; init; }
    public string? Module { get; init; }
    public string? Operation { get; init; }
    public long? ElapsedMs { get; init; }
    public object? AdditionalData { get; init; }
    public Exception? Exception { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
