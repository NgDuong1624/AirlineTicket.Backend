using System.Security.Claims;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.BuildingBlocks.Api.Extensions;
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
            .RequireAuthorization("PartnerOnly");

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
            });

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
            });

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
            });
    }

    private static bool TryGetAirlineId(ClaimsPrincipal principal, out Guid airlineId)
    {
        airlineId = Guid.Empty;
        return Guid.TryParse(principal.FindFirst("AirlineId")?.Value, out airlineId);
    }

    private static IResult Forbidden() =>
        Results.Json(new { Code = "FORBIDDEN", Message = "No airline scope on token." }, statusCode: 403);
}
