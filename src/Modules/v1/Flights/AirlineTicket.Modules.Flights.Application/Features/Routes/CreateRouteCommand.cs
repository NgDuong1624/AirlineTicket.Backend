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
    private readonly IAirportRepository _airportRepository;
    private readonly ICacheService _cacheService;

    public CreateRouteCommandHandler(IRouteRepository routeRepository, IAirportRepository airportRepository, ICacheService cacheService)
    {
        _routeRepository = routeRepository;
        _airportRepository = airportRepository;
        _cacheService = cacheService;
    }

    public async Task<Result<Guid>> Handle(CreateRouteCommand request, CancellationToken cancellationToken)
    {
        var origin = await _airportRepository.GetByIdAsync(request.OriginAirportId, cancellationToken);
        var dest = await _airportRepository.GetByIdAsync(request.DestinationAirportId, cancellationToken);
        if (origin is null || dest is null)
        {
            return Result.Failure<Guid>(new Error("Airport.NotFound", "Origin or Destination airport not found."));
        }

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
