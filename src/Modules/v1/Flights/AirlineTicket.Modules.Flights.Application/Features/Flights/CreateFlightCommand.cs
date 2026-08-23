using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record CreateFlightCommand(Guid RouteId, Guid AirplaneId, string FlightNumber, decimal BasePrice, DateTime ScheduledDeparture, DateTime ScheduledArrival) : ICommand<Result<Guid>>;

public class CreateFlightCommandHandler : ICommandHandler<CreateFlightCommand, Result<Guid>>
{
    private readonly IFlightRepository _flightRepository;
    private readonly IRouteRepository _routeRepository;

    public CreateFlightCommandHandler(IFlightRepository flightRepository, IRouteRepository routeRepository)
    {
        _flightRepository = flightRepository;
        _routeRepository = routeRepository;
    }

    public async Task<Result<Guid>> Handle(CreateFlightCommand request, CancellationToken cancellationToken)
    {
        var route = await _routeRepository.GetByIdAsync(request.RouteId, cancellationToken);
        if (route is null)
        {
            return Result.Failure<Guid>(new Error("Route.NotFound", "The specified route does not exist."));
        }

        var flight = new FlightDto
        {
            Id = Guid.NewGuid(),
            RouteId = request.RouteId,
            AirplaneId = request.AirplaneId,
            FlightNumber = request.FlightNumber,
            BasePrice = request.BasePrice,
            DepartureTime = request.ScheduledDeparture,
            ArrivalTime = request.ScheduledArrival
        };

        var id = await _flightRepository.CreateAsync(flight, cancellationToken);
        return Result.Success(id);
    }
}
