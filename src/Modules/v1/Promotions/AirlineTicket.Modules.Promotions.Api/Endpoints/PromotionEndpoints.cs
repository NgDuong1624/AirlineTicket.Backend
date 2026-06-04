using System;
using System.Threading;
using AirlineTicket.BuildingBlocks.Endpoints;
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
        // ——————————————————————— Admin Endpoints ————————————————————————————————
        var adminGroup = app.MapGroup("/api/v1/admin/promotions")
            .WithTags("Promotions Admin Manage")
            .RequireAuthorization("AdminOnly");

        adminGroup.MapPost("/", async (
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
                    return Results.Created($"/api/v1/admin/promotions/{result}", new { Id = result });
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
            .WithName("AdminCreatePromotion")
            .WithSummary("Tạo một mã giảm giá/khuyến mãi mới")
            .Produces(201)
            .Produces(400)
            .Produces(500);

        adminGroup.MapGet("/", async (
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetPromotionsAdminQuery();
                var result = await sender.Send(query, ct);
                return Results.Ok(result);
            })
            .WithName("AdminGetPromotions")
            .WithSummary("Lấy danh sách tất cả khuyến mãi (Admin)")
            .Produces(200);

        adminGroup.MapPut("/{id:guid}", async (
                Guid id,
                [FromBody] UpdatePromotionRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                try
                {
                    var command = new UpdatePromotionCommand(id, request.Name, request.DiscountValue, request.EndDate);
                    await sender.Send(command, ct);
                    return Results.Ok(new { Message = "Cập nhật khuyến mãi thành công." });
                }
                catch (AirlineTicket.BuildingBlocks.Exceptions.ValidationException ex)
                {
                    return Results.Json(new { Code = "VALIDATION_ERROR", Errors = ex.Errors }, statusCode: 400);
                }
            })
            .WithName("AdminUpdatePromotion")
            .WithSummary("Cập nhật thông tin khuyến mãi")
            .Produces(200)
            .Produces(400);

        adminGroup.MapDelete("/{id:guid}", async (
                Guid id,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new DeletePromotionCommand(id);
                await sender.Send(command, ct);
                return Results.NoContent();
            })
            .WithName("AdminDeletePromotion")
            .WithSummary("Xóa/Hủy khuyến mãi")
            .Produces(204);

        // ——————————————————————— Public Endpoints ————————————————————————————————
        var publicGroup = app.MapGroup("/api/v1/promotions")
            .WithTags("Promotions Public")
            .AllowAnonymous();

        publicGroup.MapGet("/active", async (
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetActivePromotionsQuery();
                var result = await sender.Send(query, ct);
                return Results.Ok(result);
            })
            .WithName("GetActivePromotions")
            .WithSummary("Lấy danh sách các khuyến mãi đang diễn ra")
            .Produces(200);

        publicGroup.MapGet("/{code}", async (
                string code,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetPromotionByCodeQuery(code);
                var result = await sender.Send(query, ct);
                return result != null ? Results.Ok(result) : Results.NotFound();
            })
            .WithName("GetPromotionByCode")
            .WithSummary("Lấy thông tin chi tiết một mã giảm giá")
            .Produces(200)
            .Produces(404);

        publicGroup.MapPost("/apply", async (
                [FromBody] ApplyPromotionRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                try
                {
                    var command = new ApplyPromotionCommand(request.PromoCode, request.FlightId, request.OriginalAmount);
                    var result = await sender.Send(command, ct);
                    return Results.Ok(result); // result includes DiscountAmount and FinalAmount
                }
                catch (AirlineTicket.BuildingBlocks.Exceptions.ValidationException ex)
                {
                    return Results.Json(new { Code = "VALIDATION_ERROR", Errors = ex.Errors }, statusCode: 400);
                }
                catch (Exception ex)
                {
                    return Results.Json(new { Code = "BAD_REQUEST", Message = ex.Message }, statusCode: 400);
                }
            })
            .WithName("ApplyPromotion")
            .WithSummary("Kiểm tra và tính toán giảm giá")
            .Produces(200)
            .Produces(400);
    }
}

// ======================= Requests =======================
public record CreatePromotionRequest(string Name, string PromoCode, string DiscountType, decimal DiscountValue, int MaxUsage, DateTime StartDate, DateTime EndDate);
public record UpdatePromotionRequest(string Name, decimal DiscountValue, DateTime EndDate);
public record ApplyPromotionRequest(string PromoCode, Guid FlightId, decimal OriginalAmount);
