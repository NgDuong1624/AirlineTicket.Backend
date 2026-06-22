using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AirlineTicket.BuildingBlocks.Api.Middleware;

/// <summary>
/// Middleware that logs every incoming HTTP request with duration, status code, and correlation ID.
/// Useful for observability and troubleshooting across modules.
/// When migrating to microservices, this logs can be forwarded to a centralized ingestion pipeline.
/// </summary>
public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        var correlationId = context.Response.Headers["X-Correlation-Id"].FirstOrDefault() ?? "N/A";

        _logger.LogInformation(
            "[HTTP START] [{CorrelationId}] {Method} {Path}",
            correlationId, context.Request.Method, context.Request.Path);

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            var statusCode = context.Response.StatusCode;

            if (statusCode >= 500)
            {
                _logger.LogError(
                    "[HTTP END] [{CorrelationId}] {Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
                    correlationId, context.Request.Method, context.Request.Path, statusCode, sw.ElapsedMilliseconds);
            }
            else if (statusCode >= 400)
            {
                _logger.LogWarning(
                    "[HTTP END] [{CorrelationId}] {Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
                    correlationId, context.Request.Method, context.Request.Path, statusCode, sw.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogInformation(
                    "[HTTP END] [{CorrelationId}] {Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
                    correlationId, context.Request.Method, context.Request.Path, statusCode, sw.ElapsedMilliseconds);
            }
        }
    }
}
