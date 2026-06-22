namespace AirlineTicket.BuildingBlocks.Logging;

/// <summary>
/// Application-level logging abstraction.
/// Implementations can write to Serilog, structured sinks, or forward to a centralized log service.
/// In a future microservices architecture, swap this implementation to send logs via HTTP/message queue.
/// </summary>
public interface ILoggingService
{
    void LogInformation(string message, string? module = null, string? operation = null, object? additionalData = null);
    void LogWarning(string message, string? module = null, string? operation = null, object? additionalData = null);
    void LogError(string message, Exception? exception = null, string? module = null, string? operation = null, object? additionalData = null);
    void LogDebug(string message, string? module = null, string? operation = null, object? additionalData = null);

    /// <summary>
    /// Log with full structured entry for distributed tracing scenarios.
    /// </summary>
    void Log(LogEntry entry);
}
