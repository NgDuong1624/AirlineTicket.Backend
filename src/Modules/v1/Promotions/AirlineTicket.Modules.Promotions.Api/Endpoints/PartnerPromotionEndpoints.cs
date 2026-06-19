using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using AirlineTicket.Modules.Promotions.Domain.Entities;
using AirlineTicket.Modules.Promotions.Domain.Enums;
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
                [FromServices] IPromotionRepository repo,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var items = await repo.GetCouponsByAirlineAsync(airlineId, ct);
                return Results.Ok(new { items, totalCount = items.Count });
            });

        coupons.MapPost("/", async (
                [FromBody] PartnerCouponRequest request,
                ClaimsPrincipal principal,
                [FromServices] IPromotionRepository repo,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var id = await repo.CreateCouponAsync(MapCoupon(request, airlineId), ct);
                return Results.Created($"/api/partner/coupons/{id}", new { Id = id });
            });

        coupons.MapPut("/{id:guid}", async (
                Guid id,
                [FromBody] PartnerCouponRequest request,
                ClaimsPrincipal principal,
                [FromServices] IPromotionRepository repo,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var coupon = MapCoupon(request, airlineId);
                coupon.Id = id;
                await repo.UpdateCouponAsync(coupon, airlineId, ct);
                return Results.Ok();
            });

        coupons.MapDelete("/{id:guid}", async (
                Guid id,
                ClaimsPrincipal principal,
                [FromServices] IPromotionRepository repo,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                await repo.DeleteCouponAsync(id, airlineId, ct);
                return Results.NoContent();
            });

        // ——————————————————————— Partner Campaigns ————————————————————————————————
        var campaigns = app.MapGroup("/api/partner/campaigns")
            .WithTags("Partner Campaigns")
            .RequireAuthorization("PartnerOnly");

        campaigns.MapGet("/", async (
                ClaimsPrincipal principal,
                [FromServices] IPromotionRepository repo,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var items = await repo.GetCampaignsByAirlineAsync(airlineId, ct);
                return Results.Ok(new { items, totalCount = items.Count });
            });

        campaigns.MapPost("/", async (
                [FromBody] PartnerCampaignRequest request,
                ClaimsPrincipal principal,
                [FromServices] IPromotionRepository repo,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var id = await repo.CreateCampaignAsync(new Campaign
                {
                    Title = request.Title,
                    BannerUrl = request.BannerUrl,
                    Content = request.Content,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    IsFeatured = request.IsFeatured ?? false,
                    AirlineId = airlineId
                }, ct);
                return Results.Created($"/api/partner/campaigns/{id}", new { Id = id });
            });

        campaigns.MapPut("/{id:guid}", async (
                Guid id,
                [FromBody] PartnerCampaignRequest request,
                ClaimsPrincipal principal,
                [FromServices] IPromotionRepository repo,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                await repo.UpdateCampaignAsync(new Campaign
                {
                    Id = id,
                    Title = request.Title,
                    BannerUrl = request.BannerUrl,
                    Content = request.Content,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    IsFeatured = request.IsFeatured ?? false,
                    AirlineId = airlineId
                }, ct);
                return Results.Ok();
            });

        campaigns.MapDelete("/{id:guid}", async (
                Guid id,
                [FromServices] IPromotionRepository repo,
                CancellationToken ct) =>
            {
                await repo.DeleteCampaignAsync(id, ct);
                return Results.NoContent();
            });
    }

    private static Coupon MapCoupon(PartnerCouponRequest request, Guid airlineId) => new()
    {
        Code = request.Code,
        Description = request.Description,
        DiscountType = (DiscountType)(request.DiscountType ?? 0),
        DiscountValue = request.DiscountValue,
        MinOrderValue = request.MinOrderValue,
        MaxDiscountAmount = request.MaxDiscountAmount,
        StartDate = request.StartDate,
        EndDate = request.EndDate,
        UsageLimit = request.UsageLimit,
        IsActive = request.IsActive ?? true,
        AirlineId = airlineId
    };

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
