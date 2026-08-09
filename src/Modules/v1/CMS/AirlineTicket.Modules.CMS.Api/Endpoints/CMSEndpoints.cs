using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.BuildingBlocks.Domain.Constants;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.CMS.Application.Features.Dashboard;
using AirlineTicket.Modules.CMS.Application.Features.Settings;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AirlineTicket.Modules.CMS.Api.Endpoints;

public class CMSEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // ——————————————————————— Admin Dashboard ————————————————————————————————
        // GET /api/admin/dashboard — Get admin dashboard statistics
        app.MapGet("/api/admin/dashboard", async (
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetAdminDashboardQuery();
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Error);
            })
            .WithTags("Admin Dashboard")
            .RequireAuthorization(AuthConstants.Policies.AdminOnly)
            .WithName("AdminGetDashboard")
            .WithSummary("Get admin dashboard statistics")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(403);

        // ——————————————————————— Partner Dashboard ————————————————————————————————
        // GET /api/partner/dashboard — Get partner dashboard statistics
        app.MapGet("/api/partner/dashboard", async (
                [FromServices] ISender sender,
                ClaimsPrincipal principal,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var query = new GetPartnerDashboardQuery(airlineId);
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Error);
            })
            .WithTags("Partner Dashboard")
            .RequireAuthorization(AuthConstants.Policies.PartnerOnly)
            .WithName("PartnerGetDashboard")
            .WithSummary("Get partner dashboard statistics")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(403);

        // ——————————————————————— Admin Settings ————————————————————————————————
        var adminSettings = app.MapGroup("/api/admin/settings")
            .WithTags("Admin Settings")
            .RequireAuthorization(AuthConstants.Policies.PartnerOnly);

        // GET /api/admin/settings — Get admin settings
        adminSettings.MapGet("/", async (
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetAdminSettingsQuery();
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Error);
            })
            .WithName("AdminGetSettings")
            .WithSummary("Get admin settings")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(403);

        // PUT /api/admin/settings — Update admin settings
        adminSettings.MapPut("/", async (
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new UpdateAdminSettingsCommand();
                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Error);
            })
            .WithName("AdminUpdateSettings")
            .WithSummary("Update admin settings")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(403);
    }

    private static bool TryGetAirlineId(ClaimsPrincipal principal, out Guid airlineId)
    {
        airlineId = Guid.Empty;
        var claimValue = principal.FindFirst("AirlineId")?.Value;
        return Guid.TryParse(claimValue, out airlineId);
    }

    private static IResult Forbidden() =>
        Results.Json(new { Code = "FORBIDDEN", Message = "No airline scope on token." }, statusCode: 403);
}
