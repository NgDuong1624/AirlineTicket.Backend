using AirlineTicket.BuildingBlocks.Logging;
using Microsoft.AspNetCore.Http;

namespace AirlineTicket.BuildingBlocks.Api.Middleware;

/// <summary>
/// Middleware that manages a correlation ID for each request.
/// Reads from the "X-Correlation-Id" header if present, otherwise generates one.
/// Sets the response header for client-side tracing.
/// Future microservice: forward this header to downstream services via HttpClient.
/// </summary>
public class CorrelationMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICorrelationContext correlationContext)
    {
        // Read or generate correlation ID
        var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault();
        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
        }

        correlationContext.CorrelationId = correlationId;
        correlationContext.UserId = context.User.Identity?.IsAuthenticated == true
            ? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            : null;
        correlationContext.RequestPath = $"{context.Request.Method} {context.Request.Path}";

        // Set response header for client-side tracing
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Correlation-Id"] = correlationId;
            return Task.CompletedTask;
        });

        await _next(context);
    }
}
