using AirlineTicket.BuildingBlocks.Api.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AirlineTicket.Modules.Logs.Api.Endpoints;

public class LogEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/logs")
            .WithTags("Logs Module")
            .WithOpenApi();

        group.MapGet("/", () => Results.Ok("Logs Module OK"))
            .WithName("GetLogsStatus");
    }
}
