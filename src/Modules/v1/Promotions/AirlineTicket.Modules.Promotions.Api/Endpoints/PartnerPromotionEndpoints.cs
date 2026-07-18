using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Promotions.Application.Features.Partner;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AirlineTicket.Modules.Promotions.Api.Endpoints;

public class PartnerPromotionEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // ——————————————————————— Partner Coupons ————————————————————————————————
        var coupons = app.MapGroup("/api/partner/coupons")
            .WithTags("Partner Coupons")
            .RequireAuthorization("PartnerOnly");

        coupons.MapGet("/", async (
                ClaimsPrincipal principal,
                [FromServices] ISender sender,
                CancellationToken ct,
                [FromQuery] int pageIndex = 1,
                [FromQuery] int pageSize = 10) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();

                var query = new GetCouponsPartnerQuery(airlineId, pageIndex, pageSize);
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            });

        coupons.MapPost("/", async (
                [FromBody] PartnerCouponRequest request,
                ClaimsPrincipal principal,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();

                var command = new CreateCouponPartnerCommand(
                    request.Code,
                    request.Description,
                    request.DiscountType,
                    request.DiscountValue,
                    request.MinOrderValue,
                    request.MaxDiscountAmount,
                    request.StartDate,
                    request.EndDate,
                    request.UsageLimit,
                    request.IsActive,
                    airlineId);
                var result = await sender.Send(command, ct);
                if (result.IsFailure)
                    return Results.BadRequest(result.Error);

                return Results.Created($"/api/partner/coupons/{result.Value}", new { Id = result.Value });
            });

        coupons.MapPut("/{id:guid}", async (
                Guid id,
                [FromBody] PartnerCouponRequest request,
                ClaimsPrincipal principal,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();

                var command = new UpdateCouponPartnerCommand(
                    id,
                    request.Code,
                    request.Description,
                    request.DiscountType,
                    request.DiscountValue,
                    request.MinOrderValue,
                    request.MaxDiscountAmount,
                    request.StartDate,
                    request.EndDate,
                    request.UsageLimit,
                    request.IsActive,
                    airlineId);
                var result = await sender.Send(command, ct);
                if (result.IsFailure)
                    return Results.BadRequest(result.Error);

                return Results.Ok();
            });

        coupons.MapDelete("/{id:guid}", async (
                Guid id,
                ClaimsPrincipal principal,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();

                var command = new DeleteCouponPartnerCommand(id, airlineId);
                var result = await sender.Send(command, ct);
                if (result.IsFailure)
                    return Results.BadRequest(result.Error);

                return Results.NoContent();
            });

        // ——————————————————————— Partner Campaigns ————————————————————————————————
        var campaigns = app.MapGroup("/api/partner/campaigns")
            .WithTags("Partner Campaigns")
            .RequireAuthorization("PartnerOnly");

        campaigns.MapGet("/", async (
                ClaimsPrincipal principal,
                [FromServices] ISender sender,
                CancellationToken ct,
                [FromQuery] int pageIndex = 1,
                [FromQuery] int pageSize = 10) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();

                var query = new GetCampaignsPartnerQuery(airlineId, pageIndex, pageSize);
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            });

        campaigns.MapPost("/", async (
                [FromBody] PartnerCampaignRequest request,
                ClaimsPrincipal principal,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();

                var command = new CreateCampaignPartnerCommand(
                    request.Title,
                    request.BannerUrl,
                    request.Content,
                    request.StartDate,
                    request.EndDate,
                    request.IsFeatured,
                    airlineId);
                var result = await sender.Send(command, ct);
                if (result.IsFailure)
                    return Results.BadRequest(result.Error);

                return Results.Created($"/api/partner/campaigns/{result.Value}", new { Id = result.Value });
            });

        campaigns.MapPut("/{id:guid}", async (
                Guid id,
                [FromBody] PartnerCampaignRequest request,
                ClaimsPrincipal principal,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();

                var command = new UpdateCampaignPartnerCommand(
                    id,
                    request.Title,
                    request.BannerUrl,
                    request.Content,
                    request.StartDate,
                    request.EndDate,
                    request.IsFeatured,
                    airlineId);
                var result = await sender.Send(command, ct);
                if (result.IsFailure)
                    return Results.BadRequest(result.Error);

                return Results.Ok();
            });

        campaigns.MapDelete("/{id:guid}", async (
                Guid id,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new DeleteCampaignPartnerCommand(id);
                var result = await sender.Send(command, ct);
                if (result.IsFailure)
                    return Results.BadRequest(result.Error);

                return Results.NoContent();
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

public sealed record PartnerCouponRequest(
    string Code,
    string? Description,
    int? DiscountType,
    decimal DiscountValue,
    decimal? MinOrderValue,
    decimal? MaxDiscountAmount,
    DateTime StartDate,
    DateTime EndDate,
    int? UsageLimit,
    bool? IsActive);

public sealed record PartnerCampaignRequest(
    string Title,
    string? BannerUrl,
    string? Content,
    DateTime StartDate,
    DateTime EndDate,
    bool? IsFeatured);
