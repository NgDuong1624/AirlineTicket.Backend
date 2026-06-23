using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;
using AirlineTicket.BuildingBlocks.Caching;

namespace AirlineTicket.Modules.Flights.Application.Features.Routes;

public record UpdateRouteCommand(Guid Id, Guid AirlineId, Guid OriginAirportId, Guid DestinationAirportId, decimal? DistanceKm, int? EstimatedDurationMinutes) : ICommand<Result<bool>>;

internal sealed class UpdateRouteCommandHandler : ICommandHandler<UpdateRouteCommand, Result<bool>>
{
    private readonly IRouteRepository _routeRepository;
    private readonly ICacheService _cacheService;

    public UpdateRouteCommandHandler(IRouteRepository routeRepository, ICacheService cacheService)
    {
        _routeRepository = routeRepository;
        _cacheService = cacheService;
    }

    public async Task<Result<bool>> Handle(UpdateRouteCommand request, CancellationToken cancellationToken)
    {
        var route = new Route
        {
            Id = request.Id,
            OriginAirportId = request.OriginAirportId,
            DestinationAirportId = request.DestinationAirportId,
            DistanceKm = request.DistanceKm,
            EstimatedDurationMinutes = request.EstimatedDurationMinutes
        };

        await _routeRepository.UpdateAsync(route, request.AirlineId, cancellationToken);
        
        await _cacheService.RemoveAsync(CacheKeyBuilder.ForQuery<GetRoutesQuery>("all"), cancellationToken);

        return Result.Success(true);
    }
}
