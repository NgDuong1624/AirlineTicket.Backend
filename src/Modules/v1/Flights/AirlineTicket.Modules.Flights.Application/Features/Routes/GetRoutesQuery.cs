using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.Modules.Flights.Application.Features.Routes;

public record GetRoutesQuery() : IQuery<object>;

internal sealed class GetRoutesQueryHandler : IQueryHandler<GetRoutesQuery, object>
{
    private readonly IRouteRepository _routeRepository;
    public GetRoutesQueryHandler(IRouteRepository routeRepository) => _routeRepository = routeRepository;
    public async Task<object> Handle(GetRoutesQuery request, CancellationToken cancellationToken) => await _routeRepository.GetAllAsync(cancellationToken);
}
