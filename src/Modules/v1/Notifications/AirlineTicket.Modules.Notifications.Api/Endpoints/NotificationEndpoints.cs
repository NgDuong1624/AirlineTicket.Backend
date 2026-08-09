using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.Api.Auth;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.BuildingBlocks.Auth;
using AirlineTicket.BuildingBlocks.Domain.Constants;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Notifications.Application.Features.Commands;
using AirlineTicket.Modules.Notifications.Application.Features.Queries;
using AirlineTicket.Modules.Notifications.Application.Features.Status;
using AirlineTicket.Modules.Notifications.Application.Features.Templates;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AirlineTicket.Modules.Notifications.Api.Endpoints;

public class NotificationEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notifications")
            .WithTags("Notifications Module");

        var adminGroup = app.MapGroup("/api/admin/notifications")
            .WithTags("Notifications Admin Module")
            .RequireAuthorization(AuthConstants.Policies.AdminOnly);

        // GET /api/notifications/status — Get notifications status
        group.MapGet("/status", async (
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetNotificationsStatusQuery();
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Error);
            })
            .WithName("GetNotificationsStatus")
            .WithSummary("Get notifications status")
            .Produces(200)
            .Produces(400);

        // GET /api/notifications — Get user notifications
        group.MapGet("/", async (
                [FromQuery] int pageNumber,
                [FromQuery] int pageSize,
                [FromQuery] string? locale,
                HttpContext context,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var userId = context.User.GetUserId();
                if (userId is null) return Results.Unauthorized();

                var query = new GetNotificationsQuery(userId.Value, pageNumber == 0 ? 1 : pageNumber, pageSize == 0 ? 10 : pageSize, locale ?? "en");
                var result = await sender.Send(query, ct);
                return Results.Ok(result);
            })

            .WithName("GetUserNotifications")
            .WithSummary("Get paginated notifications for the current user")
            .Produces(200)
            .Produces(401);

        // GET /api/notifications/unread-count — Get unread count
        group.MapGet("/unread-count", async (
                HttpContext context,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var userId = context.User.GetUserId();
                if (userId is null) return Results.Unauthorized();

                var query = new GetUnreadNotificationCountQuery(userId.Value);
                var count = await sender.Send(query, ct);
                return Results.Ok(new { Count = count });
            })
            .RequireAuthorization()
            .WithName("GetUnreadNotificationCount")
            .WithSummary("Get unread notification count for the current user")
            .Produces(200)
            .Produces(401);

        // PUT /api/notifications/{id}/read — Mark as read
        group.MapPut("/{id:guid}/read", async (
                Guid id,
                HttpContext context,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var userId = context.User.GetUserId();
                if (userId is null) return Results.Unauthorized();

                var command = new MarkNotificationAsReadCommand(id, userId.Value);
                var result = await sender.Send(command, ct);
                return result ? Results.NoContent() : Results.NotFound();
            })
            .RequireAuthorization()
            .WithName("MarkNotificationAsRead")
            .WithSummary("Mark a specific notification as read")
            .Produces(204)
            .Produces(401)
            .Produces(404);

        // PUT /api/notifications/read-all — Mark all as read
        group.MapPut("/read-all", async (
                HttpContext context,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var userId = context.User.GetUserId();
                if (userId is null) return Results.Unauthorized();

                var command = new MarkAllNotificationsAsReadCommand(userId.Value);
                var result = await sender.Send(command, ct);
                return result ? Results.NoContent() : Results.BadRequest();
            })
            .RequireAuthorization()
            .WithName("MarkAllNotificationsAsRead")
            .WithSummary("Mark all notifications as read for the current user")
            .Produces(204)
            .Produces(400)
            .Produces(401);

        // DELETE /api/notifications/{id} — Delete notification
        group.MapDelete("/{id:guid}", async (
                Guid id,
                HttpContext context,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var userId = context.User.GetUserId();
                if (userId is null) return Results.Unauthorized();

                var command = new DeleteNotificationCommand(id, userId.Value);
                var result = await sender.Send(command, ct);
                return result ? Results.NoContent() : Results.NotFound();
            })
            .RequireAuthorization()
            .WithName("DeleteNotification")
            .WithSummary("Soft delete a notification")
            .Produces(204)
            .Produces(401)
            .Produces(404);

        // ==========================================
        // TEMPLATE CRUD ENDPOINTS (i18n)
        // ==========================================
        var templateGroup = adminGroup.MapGroup("/templates");

        // GET /api/admin/notifications/templates — Get all templates
        templateGroup.MapGet("/", async (
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetTemplatesQuery();
                var result = await sender.Send(query, ct);
                return Results.Ok(result);
            })
            .WithName("GetTemplates")
            .WithSummary("Get all notification templates")
            .Produces(200);

        // GET /api/admin/notifications/templates/{id} — Get template by ID
        templateGroup.MapGet("/{id:guid}", async (
                Guid id,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetTemplateByIdQuery(id);
                var result = await sender.Send(query, ct);
                return result is not null ? Results.Ok(result) : Results.NotFound();
            })
            .WithName("GetTemplateById")
            .WithSummary("Get notification template by ID")
            .Produces(200)
            .Produces(404);

        // PUT /api/admin/notifications/templates/{id} — Update template
        templateGroup.MapPut("/{id:guid}", async (
                Guid id,
                [FromBody] UpdateTemplateRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var allowedLocales = new[] { "vi", "en", "zh", "ja", "ko", "fr" };
                if (Array.IndexOf(allowedLocales, request.Language.ToLower()) < 0)
                {
                    return Results.BadRequest($"Invalid language. Allowed locales: {string.Join(", ", allowedLocales)}");
                }

                var command = new UpdateTemplateCommand(id, request.Subject, request.BodyTemplate, request.Language);
                var result = await sender.Send(command, ct);
                return result ? Results.NoContent() : Results.NotFound();
            })
            .WithName("UpdateTemplate")
            .WithSummary("Update an existing notification template")
            .Produces(204)
            .Produces(400)
            .Produces(404);
    }
}

public record UpdateTemplateRequest(string Subject, string BodyTemplate, string Language);
