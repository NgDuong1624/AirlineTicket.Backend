using AirlineTicket.BuildingBlocks.Api.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AirlineTicket.Modules.CMS.Api.Endpoints;

public class CMSEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/cms")
            .WithTags("CMS Module")
            .WithOpenApi();

        group.MapGet("/", () => Results.Ok("CMS Module OK"))
            .WithName("GetCMSStatus");
    }
}
