using System;
using System.Collections.Generic;
using System.Security.Claims;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.BuildingBlocks.Api.Extensions;
using AirlineTicket.Modules.Notifications.Application.Features;
using AirlineTicket.Modules.Notifications.Application.Features.Templates;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AirlineTicket.Modules.Notifications.Api.Endpoints;

public class NotificationManagementEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/notifications/manage")
            .WithTags("Notifications Management")
            .RequireAuthorization("PartnerOrStaff");

        // GET /api/v1/notifications/manage/templates — Get all notification templates
        group.MapGet("/templates", async ([FromServices] ISender sender, CancellationToken ct) =>
        {
            var query = new GetTemplatesQuery();
            var result = await sender.Send(query, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult();
        })
        .WithName("GetNotificationTemplates")
        .WithSummary("Get all notification templates")
        .Produces(200)
        .Produces(400)
        .Produces(401)
        .Produces(403);

        // POST /api/v1/notifications/manage/templates — Create a new notification template
        group.MapPost("/templates", async ([FromServices] ISender sender, [FromBody] CreateTemplateCommand command, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess ? Results.Ok(new { id = result.Value }) : result.ToErrorResult();
        })
        .WithName("CreateNotificationTemplate")
        .WithSummary("Create a new notification template")
        .Produces(200)
        .Produces(400)
        .Produces(401)
        .Produces(403);

        // GET /api/v1/notifications/manage — Get all notifications
        group.MapGet("/", async ([FromServices] ISender sender, CancellationToken ct) =>
        {
            var query = new GetNotificationsQuery();
            var result = await sender.Send(query, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult();
        })
        .WithName("GetNotifications")
        .WithSummary("Get all notifications")
        .Produces(200)
        .Produces(400)
        .Produces(401)
        .Produces(403);
    }
}