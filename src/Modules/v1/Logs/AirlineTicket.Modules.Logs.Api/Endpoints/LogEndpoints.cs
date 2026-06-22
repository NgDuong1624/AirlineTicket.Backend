using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Logs.Application.Features;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AirlineTicket.Modules.Logs.Api.Endpoints;

public class LogEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var adminLogs = app.MapGroup("/api/admin/logs")
            .WithTags("Admin Logs")
            .RequireAuthorization("AdminOnly");

        adminLogs.MapGet("/", async (
                [FromQuery] int? pageNumber,
                [FromQuery] int? pageSize,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetAdminLogsQuery(pageNumber ?? 1, pageSize ?? 10);
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Error);
            })
            .WithName("AdminGetLogs");

        var partnerLogs = app.MapGroup("/api/partner/logs")
            .WithTags("Partner Logs")
            .RequireAuthorization("PartnerOnly");

        partnerLogs.MapGet("/", async (
                [FromQuery] int? pageNumber,
                [FromQuery] int? pageSize,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetPartnerLogsQuery(pageNumber ?? 1, pageSize ?? 10);
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Error);
            })
            .WithName("PartnerGetLogs");
    }
}
