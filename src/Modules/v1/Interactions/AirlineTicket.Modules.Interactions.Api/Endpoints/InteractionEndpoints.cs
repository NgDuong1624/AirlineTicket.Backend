using AirlineTicket.BuildingBlocks.Api.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AirlineTicket.Modules.Interactions.Api.Endpoints;

public class InteractionEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/interactions")
            .WithTags("Interactions Module")
            .WithOpenApi();

        group.MapGet("/", () => Results.Ok("Interactions Module OK"))
            .WithName("GetInteractionsStatus");
    }
}
