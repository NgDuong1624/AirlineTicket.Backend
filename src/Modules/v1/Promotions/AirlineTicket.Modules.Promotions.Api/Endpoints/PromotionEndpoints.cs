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
            .Produces(201)
            .Produces(400);

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
            .Produces(200);

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

                return Results.Ok(new { Message = "Cập nhật coupon thành công." });
            })
            .WithName("AdminUpdateCoupon")
            .Produces(200)
            .Produces(400);

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
            .Produces(204);

        // ——————————————————————— Admin Campaigns ————————————————————————————————
        var adminCampaignsGroup = app.MapGroup("/api/admin/campaigns")
            .WithTags("Admin Campaigns Management")
            .RequireAuthorization("AdminOnly");

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
            .Produces(200);

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
            .Produces(201);

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
            .Produces(200);

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
            .Produces(204);

        // ——————————————————————— Public Endpoints ————————————————————————————————
        var publicGroup = app.MapGroup("/api/promotions")
            .WithTags("Promotions Public")
            .AllowAnonymous();

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
            .Produces(200);

        publicGroup.MapGet("/campaigns", async (
                CancellationToken ct) =>
            {
                // Return campaign list
                return Results.Ok(new List<object>());
            })
            .WithName("GetCampaigns")
            .Produces(200);

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
            .Produces(200)
            .Produces(400);
    }
}

public record CreatePromotionRequest(string Name, string PromoCode, string DiscountType, decimal DiscountValue, int MaxUsage, DateTime StartDate, DateTime EndDate);
public record UpdatePromotionRequest(string Name, decimal DiscountValue, DateTime EndDate);
public record ApplyPromotionRequest(string PromoCode, Guid FlightId, decimal OriginalAmount);
public record AdminCampaignRequest(string Title, string? BannerUrl, string? Content, DateTime StartDate, DateTime EndDate, bool? IsFeatured);