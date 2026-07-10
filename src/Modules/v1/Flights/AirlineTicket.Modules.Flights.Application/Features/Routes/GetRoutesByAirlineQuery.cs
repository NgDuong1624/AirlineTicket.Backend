using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;

namespace AirlineTicket.Modules.Flights.Application.Features.Routes;

public record GetRoutesByAirlineQuery(Guid AirlineId, int PageIndex = 1, int PageSize = 10) : IQuery<PagedResult<Route>>;

internal sealed class GetRoutesByAirlineQueryHandler : IQueryHandler<GetRoutesByAirlineQuery, PagedResult<Route>>
{
    private readonly IRouteRepository _routeRepository;

    public GetRoutesByAirlineQueryHandler(IRouteRepository routeRepository)
    {
        _routeRepository = routeRepository;
    }

    public async Task<PagedResult<Route>> Handle(GetRoutesByAirlineQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _routeRepository.GetByAirlineAsync(request.AirlineId, request.PageIndex, request.PageSize, cancellationToken);
        return PagedResult<Route>.Success(items.AsReadOnly(), request.PageIndex, request.PageSize, totalCount);
    }
}
