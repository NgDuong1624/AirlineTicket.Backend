using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Notifications.Application.Features.Commands;
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
    private readonly IMediator _mediator;

    public CreatePartnerFlightCommandHandler(
        IFlightRepository flightRepository,
        IRouteRepository routeRepository,
        IAirplaneRepository airplaneRepository,
        IMediator mediator)
    {
        _flightRepository = flightRepository;
        _routeRepository = routeRepository;
        _airplaneRepository = airplaneRepository;
        _mediator = mediator;
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

        // 4. Notify staff about the new flight
        await _mediator.Send(new CreateNotificationCommand(
            UserId: null,
            TemplateCode: "FLIGHT_CREATED",
            TemplateParameters: new Dictionary<string, string>
            {
                { "FlightNumber", request.FlightNumber },
                { "Origin", route.OriginAirport.IataCode },
                { "Destination", route.DestinationAirport.IataCode },
                { "DepartureTime", request.DepartureTime.ToString("yyyy-MM-dd HH:mm:ss") },
                { "BasePrice", request.BasePrice.ToString("F2") }
            },
            Severity: 0, // Info
            ActionUrl: $"/staff/flights/{id}/seats",
            ReferenceId: id,
            ReferenceType: "Flight"
        ), cancellationToken);

        return Result.Success(id);
    }
}
