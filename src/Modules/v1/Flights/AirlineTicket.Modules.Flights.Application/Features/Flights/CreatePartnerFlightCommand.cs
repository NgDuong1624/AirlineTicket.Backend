using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record CreatePartnerFlightCommand(
    Guid AirlineId,
    Guid RouteId,
    Guid AirplaneId,
    string FlightNumber,
    decimal BasePrice,
    DateTime DepartureTime) : ICommand<Result<Guid>>;

internal sealed class CreatePartnerFlightCommandHandler : ICommandHandler<CreatePartnerFlightCommand, Result<Guid>>
{
    private readonly IFlightRepository _flightRepository;
    private readonly IRouteRepository _routeRepository;
    private readonly IAirplaneRepository _airplaneRepository;

    public CreatePartnerFlightCommandHandler(
        IFlightRepository flightRepository,
        IRouteRepository routeRepository,
        IAirplaneRepository airplaneRepository)
    {
        _flightRepository = flightRepository;
        _routeRepository = routeRepository;
        _airplaneRepository = airplaneRepository;
    }

    public async Task<Result<Guid>> Handle(CreatePartnerFlightCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate if the route belongs to the partner's airline
        var route = await _routeRepository.GetByIdAsync(request.RouteId, cancellationToken);
        if (route is null || route.AirlineId != request.AirlineId)
        {
            return Result.Failure<Guid>(new Error("ROUTE_NOT_FOUND", "The specified route does not exist or does not belong to your airline."));
        }

        if (!route.EstimatedDurationMinutes.HasValue || route.EstimatedDurationMinutes.Value <= 0)
        {
            return Result.Failure<Guid>(new Error("ROUTE_DURATION_INVALID", "The route does not have a valid estimated duration."));
        }

        // 2. Validate if the airplane belongs to the partner's airline
        var (airplanes, _) = await _airplaneRepository.GetByAirlineAsync(request.AirlineId, 1, 1000, cancellationToken);
        var airplaneExists = airplanes.Exists(a => a.Id == request.AirplaneId);
        if (!airplaneExists)
        {
            return Result.Failure<Guid>(new Error("AIRPLANE_NOT_FOUND", "The specified airplane does not exist or does not belong to your airline."));
        }

        // 3. Create the flight
        var flight = new FlightDto
        {
            Id = Guid.NewGuid(),
            RouteId = request.RouteId,
            AirplaneId = request.AirplaneId,
            FlightNumber = request.FlightNumber,
            BasePrice = request.BasePrice,
            DepartureTime = request.DepartureTime,
            ArrivalTime = request.DepartureTime.AddMinutes(route.EstimatedDurationMinutes.Value)
        };

        var id = await _flightRepository.CreateAsync(flight, cancellationToken);
        return Result.Success(id);
    }
}
