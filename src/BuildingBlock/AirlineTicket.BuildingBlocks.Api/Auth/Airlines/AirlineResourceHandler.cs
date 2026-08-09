using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.Auth;
using AirlineTicket.BuildingBlocks.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AirlineTicket.BuildingBlocks.Api.Auth.Airlines;

/// <summary>
/// Handler that restricts non-Admin users to only access resources matching their AirlineId claim.
/// Dynamically extracts "airlineId" from Route data, Query parameters, or Headers.
/// </summary>
public class AirlineResourceHandler : AuthorizationHandler<AirlineResourceRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AirlineResourceHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AirlineResourceRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        // Global Admins bypass airline boundaries
        if (context.User.IsAdmin())
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var userAirlineId = context.User.GetAirlineId();
        if (userAirlineId is null)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        // Try to locate "airlineId" in Route values
        if (httpContext.GetRouteData().Values.TryGetValue("airlineId", out var routeValue) &&
            routeValue is string routeStr &&
            Guid.TryParse(routeStr, out var routeAirlineId))
        {
            if (userAirlineId == routeAirlineId)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        // Try to locate "airlineId" in Query parameters
        if (httpContext.Request.Query.TryGetValue("airlineId", out var queryValues))
        {
            var queryStr = queryValues.FirstOrDefault();
            if (!string.IsNullOrEmpty(queryStr) && Guid.TryParse(queryStr, out var queryAirlineId))
            {
                if (userAirlineId == queryAirlineId)
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }
        }

        // Try to locate "airlineId" in HTTP Headers (useful for API keys or client headers)
        if (httpContext.Request.Headers.TryGetValue("X-Airline-Id", out var headerValues))
        {
            var headerStr = headerValues.FirstOrDefault();
            if (!string.IsNullOrEmpty(headerStr) && Guid.TryParse(headerStr, out var headerAirlineId))
            {
                if (userAirlineId == headerAirlineId)
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }
        }

        // If it got here and we didn't succeed, mark as failed
        context.Fail();
        return Task.CompletedTask;
    }
}
