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
    private readonly RequestDelegate _next;

    public SystemLogMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? string.Empty;

        // Check if it's one of the actions we want to log
        bool isCreate = HttpMethods.IsPost(method) && !path.Contains("/login", StringComparison.OrdinalIgnoreCase) && !path.Contains("/logout", StringComparison.OrdinalIgnoreCase);
        bool isUpdate = HttpMethods.IsPut(method) || HttpMethods.IsPatch(method);
        bool isDelete = HttpMethods.IsDelete(method);
        bool isLogin = path.Contains("/login", StringComparison.OrdinalIgnoreCase);
        bool isLogout = path.Contains("/logout", StringComparison.OrdinalIgnoreCase);

        bool shouldLog = isCreate || isUpdate || isDelete || isLogin || isLogout;

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
            if (shouldLog)
            {
                var systemLogService = context.RequestServices.GetService<ISystemLogService>();
                if (systemLogService != null)
                {
                    string action = isCreate ? "Create" :
                                    isUpdate ? "Update" :
                                    isDelete ? "Delete" :
                                    isLogin ? "Auth" :
                                    isLogout ? "Auth" : "Action";
                    
                    var logType = isCreate ? LogType.Create :
                                  isUpdate ? LogType.Update :
                                  isDelete ? LogType.Delete :
                                  (isLogin || isLogout) ? LogType.Auth : LogType.Create;

                    string message = $"User performed {action} on {path}";
                    string level = exception != null || (context.Response.StatusCode >= 400 && context.Response.StatusCode != 401) ? "Error" : "Info";
                    if (context.Response.StatusCode == 401) level = "Warning"; // Login failed

                    var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    Guid? userId = Guid.TryParse(userIdClaim, out var uid) ? uid : null;

                    var airlineIdClaim = context.User.FindFirst("AirlineId")?.Value;
                    Guid? airlineId = Guid.TryParse(airlineIdClaim, out var aid) ? aid : null;

                    var ipAddress = context.Connection.RemoteIpAddress?.ToString();

                    context.Request.EnableBuffering();
                    string? requestBody = null;
                    if (context.Request.ContentLength > 0)
                    {
                        using var reader = new System.IO.StreamReader(context.Request.Body, System.Text.Encoding.UTF8, leaveOpen: true);
                        requestBody = await reader.ReadToEndAsync();
                        context.Request.Body.Position = 0;
                    }

                    await systemLogService.LogAsync(
                        level: level,
                        message: message,
                        source: $"Middleware-{action}",
                        exception: exception?.ToString(),
                        userId: userId,
                        airlineId: airlineId,
                        ipAddress: ipAddress,
                        type: logType,
                        metadata: requestBody
                    );
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
