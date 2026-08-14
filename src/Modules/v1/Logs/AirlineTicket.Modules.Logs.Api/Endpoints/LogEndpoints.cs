using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.BuildingBlocks.Auth;
using AirlineTicket.BuildingBlocks.Domain.Constants;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Logs.Application.Features;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AirlineTicket.Modules.Logs.Api.Endpoints;

/// <summary>
/// Defines API endpoints for querying system logs.
/// </summary>
public class LogEndpoints : IEndpoint
{
    /// <inheritdoc />
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var adminLogs = app.MapGroup("/api/admin/logs")
            .WithTags("Admin Logs")
            .RequireAuthorization(AuthConstants.Policies.AdminOnly);

        // GET /api/admin/logs — Get system logs for admin
        adminLogs.MapGet("/", async (
                [FromServices] ISender sender,
                CancellationToken ct,
                [FromQuery] string? level,
                [FromQuery] string? search,
                [FromQuery] string? airlineId,
                [FromQuery] DateTime? date,
                [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1,
                [FromQuery, Range(1, 100)] int pageSize = 10) =>
            {
                Guid? parsedAirlineId = null;
                if (!string.IsNullOrEmpty(airlineId) && airlineId.ToLower() != "system")
                {
                    if (Guid.TryParse(airlineId, out var guid)) parsedAirlineId = guid;
                }
                var query = new GetAdminLogsQuery(pageNumber, pageSize, level, search, parsedAirlineId, null, date);
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Error);
            })
            .WithName("AdminGetLogs")
            .WithSummary("Get system logs for admin")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(403);

        var partnerLogs = app.MapGroup("/api/partner/logs")
            .WithTags("Partner Logs")
            .RequireAuthorization(AuthConstants.Policies.PartnerOnly);

        // GET /api/partner/logs — Get system logs for partner
        partnerLogs.MapGet("/", async (
                ClaimsPrincipal principal,
                [FromServices] ISender sender,
                CancellationToken ct,
                [FromQuery] string? level,
                [FromQuery] string? search,
                [FromQuery] DateTime? date,
                [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1,
                [FromQuery, Range(1, 100)] int pageSize = 10) =>
            {
                var airlineId = principal.GetAirlineId();
                if (airlineId == null)
                {
                    return Results.Json(new { Code = "FORBIDDEN", Message = "No airline scope on token." }, statusCode: 403);
                }
                var query = new GetPartnerLogsQuery(airlineId.Value, pageNumber, pageSize, level, search, date);
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Error);
            })
            .WithName("PartnerGetLogs")
            .WithSummary("Get system logs for partner")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(403);
    }
}
