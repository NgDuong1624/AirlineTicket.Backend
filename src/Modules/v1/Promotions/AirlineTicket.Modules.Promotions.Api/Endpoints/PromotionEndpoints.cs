using System;
using System.Collections.Generic;
using System.Threading;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Promotions.Application.Features.Admin;
using AirlineTicket.Modules.Promotions.Application.Features.Public;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AirlineTicket.Modules.Promotions.Api.Endpoints;

public class PromotionEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // ——————————————————————— Admin Coupons ————————————————————————————————
        var adminCouponsGroup = app.MapGroup("/api/admin/coupons")
            .WithTags("Admin Coupons Management")
            .RequireAuthorization("AdminOnly");

        // POST /api/admin/coupons — Create a new coupon
        adminCouponsGroup.MapPost("/", async (
                [FromBody] CreatePromotionRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new CreatePromotionCommand(
                    request.Name,
                    request.PromoCode,
                    request.DiscountType,
                    request.DiscountValue,
                    request.MaxUsage,
                    request.StartDate,
                    request.EndDate);

                var result = await sender.Send(command, ct);
                if (result.IsFailure)
                    return Results.BadRequest(result.Error);

                return Results.Created($"/api/admin/coupons/{result.Value}", new { Id = result.Value });
            })
            .WithName("AdminCreateCoupon")
            .WithSummary("Create a new coupon")
            .Produces(201)
            .Produces(400)
            .Produces(401)
            .Produces(403);

        // GET /api/admin/coupons — Get all coupons (paginated)
        adminCouponsGroup.MapGet("/", async (
                [FromServices] ISender sender,
                [FromQuery] int pageIndex = 1,
                [FromQuery] int pageSize = 10,
                CancellationToken ct = default) =>
            {
                var result = await sender.Send(new GetPromotionsAdminQuery(pageIndex, pageSize), ct);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            })
            .WithName("AdminGetCoupons")
            .WithSummary("Get all coupons (paginated)")
            .Produces(200)
            .Produces(401)
            .Produces(403);

        // PUT /api/admin/coupons/{id:guid} — Update a coupon
        adminCouponsGroup.MapPut("/{id:guid}", async (
                Guid id,
                [FromBody] UpdatePromotionRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new UpdatePromotionCommand(id, request.Name, request.DiscountValue, request.EndDate);
                var result = await sender.Send(command, ct);
                if (result.IsFailure)
                    return Results.BadRequest(result.Error);

                return Results.Ok(new { Message = "Coupon updated successfully." });
            })
            .WithName("AdminUpdateCoupon")
            .WithSummary("Update a coupon")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        // DELETE /api/admin/coupons/{id:guid} — Delete a coupon
        adminCouponsGroup.MapDelete("/{id:guid}", async (
                Guid id,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new DeletePromotionCommand(id);
                var result = await sender.Send(command, ct);
                if (result.IsFailure)
                    return Results.BadRequest(result.Error);

                return Results.NoContent();
            })
            .WithName("AdminDeleteCoupon")
            .WithSummary("Delete a coupon")
            .Produces(204)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        // ——————————————————————— Admin Campaigns ————————————————————————————————
        var adminCampaignsGroup = app.MapGroup("/api/admin/campaigns")
            .WithTags("Admin Campaigns Management")
            .RequireAuthorization("AdminOnly");

        // GET /api/admin/campaigns — Get all campaigns (paginated)
        adminCampaignsGroup.MapGet("/", async (
                [FromServices] ISender sender,
                [FromQuery] int pageIndex = 1,
                [FromQuery] int pageSize = 10,
                CancellationToken ct = default) =>
            {
                var result = await sender.Send(new GetCampaignsAdminQuery(pageIndex, pageSize), ct);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            })
            .WithName("AdminGetCampaigns")
            .WithSummary("Get all campaigns (paginated)")
            .Produces(200)
            .Produces(401)
            .Produces(403);

        // POST /api/admin/campaigns — Create a new campaign
        adminCampaignsGroup.MapPost("/", async (
                [FromBody] AdminCampaignRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new CreateCampaignAdminCommand(request.Title, request.BannerUrl, request.Content, request.StartDate, request.EndDate, request.IsFeatured);
                var result = await sender.Send(command, ct);
                if (result.IsFailure)
                    return Results.BadRequest(result.Error);

                return Results.Created($"/api/admin/campaigns/{result.Value}", new { Id = result.Value });
            })
            .WithName("AdminCreateCampaign")
            .WithSummary("Create a new campaign")
            .Produces(201)
            .Produces(400)
            .Produces(401)
            .Produces(403);

        // PUT /api/admin/campaigns/{id:guid} — Update a campaign
        adminCampaignsGroup.MapPut("/{id:guid}", async (
                Guid id,
                [FromBody] AdminCampaignRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new UpdateCampaignAdminCommand(id, request.Title, request.BannerUrl, request.Content, request.StartDate, request.EndDate, request.IsFeatured);
                var result = await sender.Send(command, ct);
                if (result.IsFailure)
                    return Results.BadRequest(result.Error);

                return Results.Ok();
            })
            .WithName("AdminUpdateCampaign")
            .WithSummary("Update a campaign")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        // DELETE /api/admin/campaigns/{id:guid} — Delete a campaign
        adminCampaignsGroup.MapDelete("/{id:guid}", async (
                Guid id,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new DeleteCampaignAdminCommand(id);
                var result = await sender.Send(command, ct);
                if (result.IsFailure)
                    return Results.BadRequest(result.Error);

                return Results.NoContent();
            })
            .WithName("AdminDeleteCampaign")
            .WithSummary("Delete a campaign")
            .Produces(204)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        // ——————————————————————— Public Endpoints ————————————————————————————————
        var publicGroup = app.MapGroup("/api/promotions")
            .WithTags("Promotions Public")
            .AllowAnonymous();

        // GET /api/promotions — Get active promotions
        publicGroup.MapGet("/", async (
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetActivePromotionsQuery();
                var result = await sender.Send(query, ct);
                if (result.IsFailure)
                    return Results.BadRequest(result.Error);

                return Results.Ok(result.Value);
            })
            .WithName("GetActivePromotions")
            .WithSummary("Get active promotions")
            .Produces(200)
            .Produces(400);

        // GET /api/promotions/campaigns — Get campaigns
        publicGroup.MapGet("/campaigns", async (
                CancellationToken ct) =>
            {
                // Return campaign list
                return Results.Ok(new List<object>());
            })
            .WithName("GetCampaigns")
            .WithSummary("Get campaigns")
            .Produces(200);

        // POST /api/promotions/apply — Apply a promotion code to a flight booking
        publicGroup.MapPost("/apply", async (
                [FromBody] ApplyPromotionRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new ApplyPromotionCommand(request.PromoCode, request.FlightId, request.OriginalAmount);
                var result = await sender.Send(command, ct);
                if (result.IsFailure)
                    return Results.BadRequest(result.Error);

                return Results.Ok(result.Value);
            })
            .WithName("ApplyPromotion")
            .WithSummary("Apply a promotion code to a flight booking")
            .Produces(200)
            .Produces(400);
    }
}

public record CreatePromotionRequest(string Name, string PromoCode, string DiscountType, decimal DiscountValue, int MaxUsage, DateTime StartDate, DateTime EndDate);
public record UpdatePromotionRequest(string Name, decimal DiscountValue, DateTime EndDate);
public record ApplyPromotionRequest(string PromoCode, Guid FlightId, decimal OriginalAmount);
public record AdminCampaignRequest(string Title, string? BannerUrl, string? Content, DateTime StartDate, DateTime EndDate, bool? IsFeatured);