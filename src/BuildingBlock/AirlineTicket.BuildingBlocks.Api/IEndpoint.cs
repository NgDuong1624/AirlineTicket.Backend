using Microsoft.AspNetCore.Routing;

namespace AirlineTicket.BuildingBlocks.Api.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
