using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;

namespace AirlineTicket.Modules.Flights.Application.Features.Routes;

public record CreateRouteCommand(Guid AirlineId, Guid OriginAirportId, Guid DestinationAirportId, decimal? DistanceKm, int? EstimatedDurationMinutes) : ICommand<Result<Guid>>;

internal sealed class CreateRouteCommandHandler : ICommandHandler<CreateRouteCommand, Result<Guid>>
{
    private readonly IRouteRepository _routeRepository;

    public CreateRouteCommandHandler(IRouteRepository routeRepository)
    {
        _routeRepository = routeRepository;
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
        return Result.Success(id);
    }
}
