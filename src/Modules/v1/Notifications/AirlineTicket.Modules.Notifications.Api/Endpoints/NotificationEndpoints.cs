using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Notifications.Application.Features.Status;
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
        var group = app.MapGroup("/api/v1/notifications")
            .WithTags("Notifications Module");

        group.MapGet("/", async (
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetNotificationsStatusQuery();
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Error);
            })
            .WithName("GetNotificationsStatus");
    }
}
