using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Interactions.Application.Features.Status;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AirlineTicket.Modules.Interactions.Api.Endpoints;

public class InteractionEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/interactions")
            .WithTags("Interactions Module");

        // GET /api/v1/interactions — Get interactions module status
        group.MapGet("/", async (
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetInteractionsStatusQuery();
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Error);
            })
            .WithName("GetInteractionsStatus")
            .WithSummary("Get interactions module status")
            .Produces(200)
            .Produces(400);
    }
}
