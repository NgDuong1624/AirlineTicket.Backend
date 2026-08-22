using System.Security.Claims;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.BuildingBlocks.Api.Extensions;
using AirlineTicket.BuildingBlocks.Domain.Constants;
using AirlineTicket.Modules.Bookings.Application.Features.Revenue.Queries;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AirlineTicket.Modules.Bookings.Api.Endpoints;

public class PartnerRevenueDashboardEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/partner/dashboard")
            .WithTags("Partner Dashboard")
            .RequireAuthorization(AuthConstants.Policies.PartnerOnly);

        // GET /api/partner/dashboard/sales-summary — Get sales summary for partner
        group.MapGet("/sales-summary", async (
                [FromServices] ISender sender,
                ClaimsPrincipal principal,
                [FromQuery] DateTime? fromDate,
                [FromQuery] DateTime? toDate,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var query = new GetSalesSummaryQuery(airlineId, fromDate ?? DateTime.UtcNow.AddDays(-30), toDate ?? DateTime.UtcNow);
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult();
            })
            .WithSummary("Get sales summary for partner")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(403);

        // GET /api/partner/dashboard/occupancy-rates — Get occupancy rates for partner
        group.MapGet("/occupancy-rates", async (
                [FromServices] ISender sender,
                ClaimsPrincipal principal,
                [FromQuery] DateTime? fromDate,
                [FromQuery] DateTime? toDate,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var query = new GetOccupancyRatesQuery(airlineId, fromDate ?? DateTime.UtcNow.AddDays(-30), toDate ?? DateTime.UtcNow);
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult();
            })
            .WithSummary("Get occupancy rates for partner")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(403);

        // GET /api/partner/dashboard/revenue-trends — Get revenue trends for partner
        group.MapGet("/revenue-trends", async (
                [FromServices] ISender sender,
                ClaimsPrincipal principal,
                [FromQuery] DateTime? fromDate,
                [FromQuery] DateTime? toDate,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var query = new GetRevenueTrendsQuery(airlineId, fromDate ?? DateTime.UtcNow.AddDays(-30), toDate ?? DateTime.UtcNow);
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult();
            })
            .WithSummary("Get revenue trends for partner")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(403);
    }

    private static bool TryGetAirlineId(ClaimsPrincipal principal, out Guid airlineId)
    {
        airlineId = Guid.Empty;
        return Guid.TryParse(principal.FindFirst("AirlineId")?.Value, out airlineId);
    }

    private static IResult Forbidden() =>
        new Error("Common.Forbidden", "No airline scope on token.").ToErrorResult(statusCode: 403);
}
