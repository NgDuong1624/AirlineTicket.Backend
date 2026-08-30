using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.Application.Events;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;
using MediatR;

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
    private readonly IPublisher _publisher;

    public CreatePartnerFlightCommandHandler(
        IFlightRepository flightRepository,
        IRouteRepository routeRepository,
        IAirplaneRepository airplaneRepository,
        IPublisher publisher)
    {
        _flightRepository = flightRepository;
        _routeRepository = routeRepository;
        _airplaneRepository = airplaneRepository;
        _publisher = publisher;
    }

    public async Task<Result<Guid>> Handle(CreatePartnerFlightCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate if the route belongs to the partner's airline
        var route = await _routeRepository.GetByIdAsync(request.RouteId, cancellationToken);
        if (route is null || route.AirlineId != request.AirlineId)
        {
            return Result.Failure<Guid>(new Error("Route.NotFound", "The specified route does not exist or does not belong to your airline."));
        }

        if (!route.EstimatedDurationMinutes.HasValue || route.EstimatedDurationMinutes.Value <= 0)
        {
            return Result.Failure<Guid>(new Error("Route.DurationInvalid", "The route does not have a valid estimated duration."));
        }

        // 2. Validate if the airplane belongs to the partner's airline
        var (airplanes, _) = await _airplaneRepository.GetByAirlineAsync(request.AirlineId, 1, 1000, cancellationToken);
        var airplaneExists = airplanes.Exists(a => a.Id == request.AirplaneId);
        if (!airplaneExists)
        {
            return Result.Failure<Guid>(new Error("Airplane.NotFound", "The specified airplane does not exist or does not belong to your airline."));
        }

        // 3. Create the flight
        var arrivalTime = request.DepartureTime.AddMinutes(route.EstimatedDurationMinutes.Value);
        var flight = new FlightDto
        {
            Id = Guid.NewGuid(),
            RouteId = request.RouteId,
            AirplaneId = request.AirplaneId,
            FlightNumber = request.FlightNumber,
            BasePrice = request.BasePrice,
            DepartureTime = request.DepartureTime,
            ArrivalTime = arrivalTime
        };

        var id = await _flightRepository.CreateAsync(flight, cancellationToken);

        // 4. Publish event for cross-module handling (notifications & real-time broadcast)
        await _publisher.Publish(new FlightCreatedEvent(
            id,
            request.RouteId,
            request.AirplaneId,
            request.FlightNumber,
            request.BasePrice,
            request.DepartureTime,
            arrivalTime,
            request.AirlineId
        ), cancellationToken);

        return Result.Success(id);
    }
}
