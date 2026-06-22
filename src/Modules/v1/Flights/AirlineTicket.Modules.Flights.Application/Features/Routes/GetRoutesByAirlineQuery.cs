using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;

namespace AirlineTicket.Modules.Flights.Application.Features.Routes;

public record GetRoutesByAirlineQuery(Guid AirlineId) : IQuery<Result<List<Route>>>;

internal sealed class GetRoutesByAirlineQueryHandler : IQueryHandler<GetRoutesByAirlineQuery, Result<List<Route>>>
{
    private readonly IRouteRepository _routeRepository;

    public GetRoutesByAirlineQueryHandler(IRouteRepository routeRepository)
    {
        _routeRepository = routeRepository;
    }

    public async Task<Result<List<Route>>> Handle(GetRoutesByAirlineQuery request, CancellationToken cancellationToken)
    {
        var routes = await _routeRepository.GetByAirlineAsync(request.AirlineId, cancellationToken);
        return Result.Success(routes);
    }
}
