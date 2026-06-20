using System;
using System.Threading;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
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
                try
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
                    return Results.Created($"/api/admin/coupons/{result}", new { Id = result });
                }
                catch (AirlineTicket.BuildingBlocks.Exceptions.ValidationException ex)
                {
                    return Results.Json(new { Code = "VALIDATION_ERROR", Errors = ex.Errors }, statusCode: 400);
                }
                catch (Exception ex)
                {
                    return Results.Json(new { Code = "INTERNAL_ERROR", Message = ex.Message }, statusCode: 500);
                }
            })
            .WithName("AdminCreateCoupon")
            .Produces(201)
            .Produces(400);

        adminCouponsGroup.MapGet("/", async (
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetPromotionsAdminQuery();
                var result = await sender.Send(query, ct);
                return Results.Ok(new { items = result, totalCount = (result as System.Collections.IList)?.Count ?? 0 });
            })
            .WithName("AdminGetCoupons")
            .Produces(200);

        adminCouponsGroup.MapPut("/{id:guid}", async (
                Guid id,
                [FromBody] UpdatePromotionRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                try
                {
                    var command = new UpdatePromotionCommand(id, request.Name, request.DiscountValue, request.EndDate);
                    await sender.Send(command, ct);
                    return Results.Ok(new { Message = "Cập nhật coupon thành công." });
                }
                catch (Exception ex)
                {
                    return Results.Json(new { Code = "BAD_REQUEST", Message = ex.Message }, statusCode: 400);
                }
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
                await sender.Send(command, ct);
                return Results.NoContent();
            })
            .WithName("AdminDeleteCoupon")
            .Produces(204);

        // ——————————————————————— Admin Campaigns ————————————————————————————————
        var adminCampaignsGroup = app.MapGroup("/api/admin/campaigns")
            .WithTags("Admin Campaigns Management")
            .RequireAuthorization("AdminOnly");

        adminCampaignsGroup.MapGet("/", async (
                [FromServices] AirlineTicket.Modules.Promotions.Application.Contracts.IPromotionRepository repo,
                CancellationToken ct) =>
            {
                var items = await repo.GetAllCampaignsAsync(ct);
                return Results.Ok(new { items, totalCount = items.Count });
            })
            .WithName("AdminGetCampaigns")
            .Produces(200);

        adminCampaignsGroup.MapPost("/", async (
                [FromBody] AdminCampaignRequest request,
                [FromServices] AirlineTicket.Modules.Promotions.Application.Contracts.IPromotionRepository repo,
                CancellationToken ct) =>
            {
                var id = await repo.CreateCampaignAsync(new AirlineTicket.Modules.Promotions.Domain.Entities.Campaign
                {
                    Title = request.Title,
                    BannerUrl = request.BannerUrl,
                    Content = request.Content,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    IsFeatured = request.IsFeatured ?? false
                }, ct);
                return Results.Created($"/api/admin/campaigns/{id}", new { Id = id });
            })
            .WithName("AdminCreateCampaign")
            .Produces(201);

        adminCampaignsGroup.MapPut("/{id:guid}", async (
                Guid id,
                [FromBody] AdminCampaignRequest request,
                [FromServices] AirlineTicket.Modules.Promotions.Application.Contracts.IPromotionRepository repo,
                CancellationToken ct) =>
            {
                await repo.UpdateCampaignAsync(new AirlineTicket.Modules.Promotions.Domain.Entities.Campaign
                {
                    Id = id,
                    Title = request.Title,
                    BannerUrl = request.BannerUrl,
                    Content = request.Content,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    IsFeatured = request.IsFeatured ?? false
                }, ct);
                return Results.Ok();
            })
            .WithName("AdminUpdateCampaign")
            .Produces(200);

        adminCampaignsGroup.MapDelete("/{id:guid}", async (
                Guid id,
                [FromServices] AirlineTicket.Modules.Promotions.Application.Contracts.IPromotionRepository repo,
                CancellationToken ct) =>
            {
                await repo.DeleteCampaignAsync(id, ct);
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
                return Results.Ok(result);
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
                try
                {
                    var command = new ApplyPromotionCommand(request.PromoCode, request.FlightId, request.OriginalAmount);
                    var result = await sender.Send(command, ct);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.Json(new { Code = "BAD_REQUEST", Message = ex.Message }, statusCode: 400);
                }
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