using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using AirlineTicket.BuildingBlocks.Logging;
using AirlineTicket.BuildingBlocks.Domain.Enums;

namespace AirlineTicket.BuildingBlocks.Api.Middleware;

public class SystemLogMiddleware
{
    private const string MiddlewareErrorSource = "Middleware-Error";
    private const string AirlineIdClaimType = "AirlineId";
    private const string ErrorLogLevel = "Error";

    private readonly RequestDelegate _next;

    public SystemLogMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        Exception? exception = null;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            exception = ex;
            throw;
        }
        finally
        {
            if (exception != null || context.Response.StatusCode >= 500)
            {
                try
                {
                    var systemLogService = context.RequestServices.GetService<ISystemLogService>();
                    if (systemLogService != null)
                    {
                        var path = context.Request.Path.Value ?? string.Empty;
                        string message = exception != null
                            ? $"Unhandled exception: {exception.Message}"
                            : $"Server error: {context.Response.StatusCode} at {path}";

                        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        Guid? userId = Guid.TryParse(userIdClaim, out var uid) ? uid : null;

                        var airlineIdClaim = context.User.FindFirst(AirlineIdClaimType)?.Value;
                        Guid? airlineId = Guid.TryParse(airlineIdClaim, out var aid) ? aid : null;

                        var ipAddress = context.Connection.RemoteIpAddress?.ToString();

                        await systemLogService.LogAsync(
                            level: ErrorLogLevel,
                            message: message,
                            source: MiddlewareErrorSource,
                            exception: exception?.ToString(),
                            userId: userId,
                            airlineId: airlineId,
                            ipAddress: ipAddress,
                            type: LogType.SystemError,
                            metadata: null
                        );
                    }
                }
                catch
                {
                    // Prevent logging failure from masking original exception
                }
            }
        }
    }
}

public static class SystemLogMiddlewareExtensions
{
    public static IApplicationBuilder UseSystemLogMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SystemLogMiddleware>();
    }
}
