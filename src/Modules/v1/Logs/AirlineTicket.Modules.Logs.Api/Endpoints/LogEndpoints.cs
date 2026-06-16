using AirlineTicket.BuildingBlocks.Api.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;

namespace AirlineTicket.Modules.Logs.Api.Endpoints;

public class LogEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var adminLogs = app.MapGroup("/api/admin/logs")
            .WithTags("Admin Logs")
            .RequireAuthorization("AdminOnly");

        adminLogs.MapGet("/", () => Results.Ok(new List<object>()))
            .WithName("AdminGetLogs");
    }
}