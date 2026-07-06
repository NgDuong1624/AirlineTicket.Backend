using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record UpdatePartnerFlightCommand(
    Guid Id,
    Guid AirlineId,
    Guid RouteId,
    Guid AirplaneId,
    string FlightNumber,
    decimal BasePrice,
    DateTime ScheduledDeparture,
    DateTime ScheduledArrival) : ICommand<Result<bool>>;

internal sealed class UpdatePartnerFlightCommandHandler : ICommandHandler<UpdatePartnerFlightCommand, Result<bool>>
{
    private readonly IFlightRepository _flightRepository;

    public UpdatePartnerFlightCommandHandler(IFlightRepository flightRepository)
    {
        _flightRepository = flightRepository;
    }

    public async Task<Result<bool>> Handle(UpdatePartnerFlightCommand request, CancellationToken cancellationToken)
    {
        var flightDto = new FlightDto
        {
            Id = request.Id,
            RouteId = request.RouteId,
            AirplaneId = request.AirplaneId,
            FlightNumber = request.FlightNumber,
            BasePrice = request.BasePrice,
            DepartureTime = request.ScheduledDeparture,
            ArrivalTime = request.ScheduledArrival
        };

        await _flightRepository.UpdateAsync(flightDto, request.AirlineId, cancellationToken);
        return Result.Success(true);
    }
}
