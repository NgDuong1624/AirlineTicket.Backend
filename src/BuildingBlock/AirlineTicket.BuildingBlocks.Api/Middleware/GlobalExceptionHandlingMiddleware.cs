using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.Application.Localization;
using AirlineTicket.BuildingBlocks.Exceptions;
using AirlineTicket.BuildingBlocks.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AirlineTicket.BuildingBlocks.Api.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred during request execution: {Message}", ex.Message);
            await HandleGenericExceptionAsync(context, ex);
        }
    }

    private static async Task HandleValidationExceptionAsync(HttpContext context, ValidationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";

        var localizer = context.RequestServices.GetService<IErrorLocalizer>();

        var localizedMessage = localizer != null
            ? localizer.Localize("VALIDATION_FAILED", ex.Message)
            : ex.Message;

        var localizedErrors = new Dictionary<string, string[]>();

        if (ex.FailureInfos != null && ex.FailureInfos.Count > 0)
        {
            foreach (var group in ex.FailureInfos.GroupBy(x => x.PropertyName))
            {
                var propertyName = group.Key;
                var translatedList = new List<string>();

                foreach (var info in group)
                {
                    if (localizer != null)
                    {
                        var translated = localizer.Localize(info.ErrorCode, fallbackMessage: info.ErrorMessage, args: info.CustomArgs);
                        translatedList.Add(translated);
                    }
                    else
                    {
                        translatedList.Add(info.ErrorMessage);
                    }
                }

                localizedErrors[propertyName] = translatedList.ToArray();
            }
        }
        else if (ex.Errors != null)
        {
            foreach (var kvp in ex.Errors)
            {
                var propertyName = kvp.Key;
                var translatedList = new List<string>();

                foreach (var err in kvp.Value)
                {
                    if (localizer != null)
                    {
                        var translatedErr = localizer.Localize(err, fallbackMessage: err, args: [propertyName]);
                        translatedList.Add(translatedErr);
                    }
                    else
                    {
                        translatedList.Add(err);
                    }
                }

                localizedErrors[propertyName] = translatedList.ToArray();
            }
        }

        var response = new ErrorResponse("VALIDATION_FAILED", localizedMessage, localizedErrors);
        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }

    private static async Task HandleGenericExceptionAsync(HttpContext context, Exception ex)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var localizer = context.RequestServices.GetService<IErrorLocalizer>();
        var localizedMessage = localizer != null
            ? localizer.Localize("INTERNAL_SERVER_ERROR", "An unexpected error occurred. Please try again later.")
            : "An unexpected error occurred. Please try again later.";

        var response = new ErrorResponse("INTERNAL_SERVER_ERROR", localizedMessage);
        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}

public static class GlobalExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandlingMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    }
}
