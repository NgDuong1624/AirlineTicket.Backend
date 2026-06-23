using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;
using AirlineTicket.BuildingBlocks.Caching;

namespace AirlineTicket.Modules.Flights.Application.Features.Routes;

public record CreateRouteCommand(Guid AirlineId, Guid OriginAirportId, Guid DestinationAirportId, decimal? DistanceKm, int? EstimatedDurationMinutes) : ICommand<Result<Guid>>;

internal sealed class CreateRouteCommandHandler : ICommandHandler<CreateRouteCommand, Result<Guid>>
{
    private readonly IRouteRepository _routeRepository;
    private readonly ICacheService _cacheService;

    public CreateRouteCommandHandler(IRouteRepository routeRepository, ICacheService cacheService)
    {
        _routeRepository = routeRepository;
        _cacheService = cacheService;
    }

    public async Task<Result<Guid>> Handle(CreateRouteCommand request, CancellationToken cancellationToken)
    {
        var route = new Route
        {
            AirlineId = request.AirlineId,
            OriginAirportId = request.OriginAirportId,
            DestinationAirportId = request.DestinationAirportId,
            DistanceKm = request.DistanceKm,
            EstimatedDurationMinutes = request.EstimatedDurationMinutes
        };

        var id = await _routeRepository.CreateAsync(route, cancellationToken);
        
        await _cacheService.RemoveAsync(CacheKeyBuilder.ForQuery<GetRoutesQuery>("all"), cancellationToken);

        return Result.Success(id);
    }
}
