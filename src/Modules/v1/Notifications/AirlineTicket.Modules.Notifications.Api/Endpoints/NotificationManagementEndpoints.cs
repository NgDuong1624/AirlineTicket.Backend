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

        group.MapGet("/templates", async ([FromServices] ISender sender, CancellationToken ct) =>
        {
            var query = new GetTemplatesQuery();
            var result = await sender.Send(query, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult();
        });

        group.MapPost("/templates", async ([FromServices] ISender sender, [FromBody] CreateTemplateCommand command, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess ? Results.Ok(new { id = result.Value }) : result.ToErrorResult();
        });

        group.MapGet("/", async ([FromServices] ISender sender, CancellationToken ct) =>
        {
            var query = new GetNotificationsQuery();
            var result = await sender.Send(query, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult();
        });
    }
}