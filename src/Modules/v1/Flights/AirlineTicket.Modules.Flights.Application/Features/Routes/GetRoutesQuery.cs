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

public record GetRoutesQuery(int PageIndex = 1, int PageSize = 100) : IQuery<Result<PagedResult<Route>>>, ICacheableRequest
{
    public string CacheKey => CacheKeyBuilder.ForQuery<GetRoutesQuery>($"{PageIndex}_{PageSize}");
    public int CacheDurationMinutes => 30;
}

internal sealed class GetRoutesQueryHandler : IQueryHandler<GetRoutesQuery, Result<PagedResult<Route>>>
{
    private readonly IRouteRepository _routeRepository;

    public GetRoutesQueryHandler(IRouteRepository routeRepository)
    {
        _routeRepository = routeRepository;
    }

    public async Task<Result<PagedResult<Route>>> Handle(GetRoutesQuery request, CancellationToken cancellationToken)
    {
        var routes = await _routeRepository.GetAllAsync(cancellationToken);
        var totalCount = routes.Count;
        var items = routes
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();
        return Result.Success(PagedResult<Route>.Success(items, request.PageIndex, request.PageSize, totalCount));
    }
}
