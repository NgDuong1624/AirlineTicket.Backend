using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.BuildingBlocks.Behaviors;
using AirlineTicket.BuildingBlocks.Caching;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;

namespace AirlineTicket.Modules.Flights.Application.Features.Routes;

public record GetRoutesQuery() : IQuery<Result<List<Route>>>, ICacheableRequest
{
    public string CacheKey => CacheKeyBuilder.ForQuery<GetRoutesQuery>("all");
    public int CacheDurationMinutes => 30;
}

internal sealed class GetRoutesQueryHandler : IQueryHandler<GetRoutesQuery, Result<List<Route>>>
{
    private readonly IRouteRepository _routeRepository;

    public GetRoutesQueryHandler(IRouteRepository routeRepository)
    {
        _routeRepository = routeRepository;
    }

    public async Task<Result<List<Route>>> Handle(GetRoutesQuery request, CancellationToken cancellationToken)
    {
        var routes = await _routeRepository.GetAllAsync(cancellationToken);
        return Result.Success(routes);
    }
}
