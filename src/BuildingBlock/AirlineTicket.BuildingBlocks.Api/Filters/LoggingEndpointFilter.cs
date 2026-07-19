using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using AirlineTicket.BuildingBlocks.Logging;
using AirlineTicket.BuildingBlocks.Domain.Enums;

namespace AirlineTicket.BuildingBlocks.Api.Filters;

public class LoggingEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var method = httpContext.Request.Method;
        var path = httpContext.Request.Path.Value ?? string.Empty;

        // Check if it's one of the actions we want to log
        bool isCreate = HttpMethods.IsPost(method) && !path.Contains("/login", StringComparison.OrdinalIgnoreCase) && !path.Contains("/logout", StringComparison.OrdinalIgnoreCase);
        bool isUpdate = HttpMethods.IsPut(method) || HttpMethods.IsPatch(method);
        bool isDelete = HttpMethods.IsDelete(method);
        bool isLogin = path.Contains("/login", StringComparison.OrdinalIgnoreCase);
        bool isLogout = path.Contains("/logout", StringComparison.OrdinalIgnoreCase);

        bool shouldLog = isCreate || isUpdate || isDelete || isLogin || isLogout;

        object? result = null;
        Exception? exception = null;

        try
        {
            // Execute the endpoint
            result = await next(context);
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
                var systemLogService = httpContext.RequestServices.GetService<ISystemLogService>();
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
                    string level = exception != null || (httpContext.Response.StatusCode >= 400 && httpContext.Response.StatusCode != 401) ? "Error" : "Info";
                    if (httpContext.Response.StatusCode == 401) level = "Warning"; // Login failed

                    var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    Guid? userId = Guid.TryParse(userIdClaim, out var uid) ? uid : null;

                    var airlineIdClaim = httpContext.User.FindFirst("AirlineId")?.Value;
                    Guid? airlineId = Guid.TryParse(airlineIdClaim, out var aid) ? aid : null;

                    var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

                    await systemLogService.LogAsync(
                        level: level,
                        message: message,
                        source: $"EndpointFilter-{action}",
                        exception: exception?.ToString(),
                        userId: userId,
                        airlineId: airlineId,
                        ipAddress: ipAddress,
                        type: logType,
                        metadata: null
                    );
                }
            }
        }

        return result;
    }
}
