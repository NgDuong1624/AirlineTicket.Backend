using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.BuildingBlocks.Domain.Constants;
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
            .RequireAuthorization(AuthConstants.Policies.PartnerOnly);

        // GET /api/partner/coupons — Get partner coupons (paginated)
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
            })
            .WithName("GetPartnerCoupons")
            .WithSummary("Get partner coupons (paginated)")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(403);

        // POST /api/partner/coupons — Create a new partner coupon
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
            })
            .WithName("CreatePartnerCoupon")
            .WithSummary("Create a new partner coupon")
            .Produces(201)
            .Produces(400)
            .Produces(401)
            .Produces(403);

        // PUT /api/partner/coupons/{id:guid} — Update a partner coupon
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
            })
            .WithName("UpdatePartnerCoupon")
            .WithSummary("Update a partner coupon")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        // DELETE /api/partner/coupons/{id:guid} — Delete a partner coupon
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
            })
            .WithName("DeletePartnerCoupon")
            .WithSummary("Delete a partner coupon")
            .Produces(204)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        // ——————————————————————— Partner Campaigns ————————————————————————————————
        var campaigns = app.MapGroup("/api/partner/campaigns")
            .WithTags("Partner Campaigns")
            .RequireAuthorization(AuthConstants.Policies.PartnerOnly);

        // GET /api/partner/campaigns — Get partner campaigns (paginated)
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
            })
            .WithName("GetPartnerCampaigns")
            .WithSummary("Get partner campaigns (paginated)")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(403);

        // POST /api/partner/campaigns — Create a new partner campaign
        campaigns.MapPost("/", async (
                [FromBody] PartnerCampaignRequest request,
                ClaimsPrincipal principal,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();

                var command = new CreateCampaignPartnerCommand(
                    request.TitleEn,
                    request.TitleVi,
                    request.BannerUrl,
                    request.ContentEn,
                    request.ContentVi,
                    request.StartDate,
                    request.EndDate,
                    request.IsFeatured,
                    airlineId);
                var result = await sender.Send(command, ct);
                if (result.IsFailure)
                    return Results.BadRequest(result.Error);

                return Results.Created($"/api/partner/campaigns/{result.Value}", new { Id = result.Value });
            })
            .WithName("CreatePartnerCampaign")
            .WithSummary("Create a new partner campaign")
            .Produces(201)
            .Produces(400)
            .Produces(401)
            .Produces(403);

        // PUT /api/partner/campaigns/{id:guid} — Update a partner campaign
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
                    request.TitleEn,
                    request.TitleVi,
                    request.BannerUrl,
                    request.ContentEn,
                    request.ContentVi,
                    request.StartDate,
                    request.EndDate,
                    request.IsFeatured,
                    airlineId);
                var result = await sender.Send(command, ct);
                if (result.IsFailure)
                    return Results.BadRequest(result.Error);

                return Results.Ok();
            })
            .WithName("UpdatePartnerCampaign")
            .WithSummary("Update a partner campaign")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        // DELETE /api/partner/campaigns/{id:guid} — Delete a partner campaign
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
            })
            .WithName("DeletePartnerCampaign")
            .WithSummary("Delete a partner campaign")
            .Produces(204)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);
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
    string TitleEn,
    string TitleVi,
    string? BannerUrl,
    string? ContentEn,
    string? ContentVi,
    DateTime StartDate,
    DateTime EndDate,
    bool? IsFeatured);
