using AirlineTicket.BuildingBlocks.Logging;
using Microsoft.Extensions.Logging;

namespace AirlineTicket.BuildingBlocks.Infrastructure.Logging;

/// <summary>
/// Application-level logging service backed by Microsoft.Extensions.Logging (Serilog).
/// Correlates all log entries with the current request correlation ID for distributed tracing.
/// When moving to microservices, swap the ICorrelationContext source to accept W3C trace headers.
/// </summary>
public class LoggingService : ILoggingService
{
    private readonly ILogger<LoggingService> _logger;
    private readonly ICorrelationContext _correlationContext;

    public LoggingService(ILogger<LoggingService> logger, ICorrelationContext correlationContext)
    {
        _logger = logger;
        _correlationContext = correlationContext;
    }

    public void LogInformation(string message, string? module = null, string? operation = null, object? additionalData = null)
    {
        _logger.LogInformation(
            "[{Module}] [{Operation}] [Correlation: {CorrelationId}] [User: {UserId}] {Message} {Data}",
            module, operation, _correlationContext.CorrelationId, _correlationContext.UserId, message, additionalData ?? "");
    }

    public void LogWarning(string message, string? module = null, string? operation = null, object? additionalData = null)
    {
        _logger.LogWarning(
            "[{Module}] [{Operation}] [Correlation: {CorrelationId}] [User: {UserId}] {Message} {Data}",
            module, operation, _correlationContext.CorrelationId, _correlationContext.UserId, message, additionalData ?? "");
    }

    public void LogError(string message, Exception? exception = null, string? module = null, string? operation = null, object? additionalData = null)
    {
        _logger.LogError(
            exception,
            "[{Module}] [{Operation}] [Correlation: {CorrelationId}] [User: {UserId}] {Message} {Data}",
            module, operation, _correlationContext.CorrelationId, _correlationContext.UserId, message, additionalData ?? "");
    }

    public void LogDebug(string message, string? module = null, string? operation = null, object? additionalData = null)
    {
        _logger.LogDebug(
            "[{Module}] [{Operation}] [Correlation: {CorrelationId}] [User: {UserId}] {Message} {Data}",
            module, operation, _correlationContext.CorrelationId, _correlationContext.UserId, message, additionalData ?? "");
    }

    public void Log(LogEntry entry)
    {
        // Use the correlation context values as fallback if the entry lacks them
        var correlationId = !string.IsNullOrEmpty(entry.CorrelationId) ? entry.CorrelationId : _correlationContext.CorrelationId;
        var userId = entry.UserId ?? _correlationContext.UserId;

        switch (entry.Level?.ToLowerInvariant())
        {
            case "error":
                _logger.LogError(entry.Exception,
                    "[{Module}] [{Operation}] [Correlation: {CorrelationId}] [User: {UserId}] [{ElapsedMs}ms] {Message} {Data}",
                    entry.Module, entry.Operation, correlationId, userId, entry.ElapsedMs, entry.Message, entry.AdditionalData ?? "");
                break;
            case "warning":
                _logger.LogWarning(
                    "[{Module}] [{Operation}] [Correlation: {CorrelationId}] [User: {UserId}] [{ElapsedMs}ms] {Message} {Data}",
                    entry.Module, entry.Operation, correlationId, userId, entry.ElapsedMs, entry.Message, entry.AdditionalData ?? "");
                break;
            default:
                _logger.LogInformation(
                    "[{Module}] [{Operation}] [Correlation: {CorrelationId}] [User: {UserId}] [{ElapsedMs}ms] {Message} {Data}",
                    entry.Module, entry.Operation, correlationId, userId, entry.ElapsedMs, entry.Message, entry.AdditionalData ?? "");
                break;
        }
    }
}
