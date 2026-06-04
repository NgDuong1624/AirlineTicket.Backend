using Microsoft.AspNetCore.Routing;

namespace AirlineTicket.BuildingBlocks.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
