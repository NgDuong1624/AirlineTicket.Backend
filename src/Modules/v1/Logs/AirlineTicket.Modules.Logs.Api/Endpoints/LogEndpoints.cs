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

        adminLogs.MapGet("/", () => Results.Ok(new { items = new List<object>(), totalCount = 0 }))
            .WithName("AdminGetLogs");

        var partnerLogs = app.MapGroup("/api/partner/logs")
            .WithTags("Partner Logs")
            .RequireAuthorization("PartnerOnly");

        partnerLogs.MapGet("/", () => Results.Ok(new { items = new List<object>(), totalCount = 0 }))
            .WithName("PartnerGetLogs");
    }
}