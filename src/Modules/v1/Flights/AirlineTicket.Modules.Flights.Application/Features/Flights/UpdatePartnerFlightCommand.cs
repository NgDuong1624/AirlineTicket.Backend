using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Enums;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record UpdatePartnerFlightCommand(
    Guid Id,
    Guid AirlineId,
    DateTime DepartureTime) : ICommand<Result<bool>>;

internal sealed class UpdatePartnerFlightCommandHandler : ICommandHandler<UpdatePartnerFlightCommand, Result<bool>>
{
    private readonly IFlightRepository _flightRepository;
    private readonly IRouteRepository _routeRepository;

    public UpdatePartnerFlightCommandHandler(IFlightRepository flightRepository, IRouteRepository routeRepository)
    {
        _flightRepository = flightRepository;
        _routeRepository = routeRepository;
    }

    public async Task<Result<bool>> Handle(UpdatePartnerFlightCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch existing flight
        var existing = await _flightRepository.GetByIdAsync(request.Id, cancellationToken);
        if (existing is null)
        {
            return Result.Failure<bool>(new Error("FLIGHT_NOT_FOUND", "Flight not found."));
        }

        // 2. Verify airline ownership
        if (existing.AirlineId != request.AirlineId)
        {
            return Result.Failure<bool>(new Error("FORBIDDEN", "This flight does not belong to your airline."));
        }

        // 3. Validate flight status
        var status = (FlightStatus)existing.Status;
        if (status == FlightStatus.Cancelled)
        {
            return Result.Failure<bool>(new Error("FLIGHT_CANCELLED", "Cannot edit a cancelled flight."));
        }

        if (status != FlightStatus.Scheduled && status != FlightStatus.Delayed)
        {
            return Result.Failure<bool>(new Error("FLIGHT_NOT_EDITABLE", "Only Scheduled or Delayed flights can be updated."));
        }

        // 4. Fetch route to calculate arrival time
        var route = await _routeRepository.GetByIdAsync(existing.RouteId, cancellationToken);
        if (route is null || !route.EstimatedDurationMinutes.HasValue || route.EstimatedDurationMinutes.Value <= 0)
        {
            return Result.Failure<bool>(new Error("ROUTE_DURATION_INVALID", "Route not found or has invalid estimated duration."));
        }

        // 5. Determine new status: if Scheduled and departure pushed later → Delayed
        var newStatus = status;
        if (status == FlightStatus.Scheduled && request.DepartureTime > existing.DepartureTime)
        {
            newStatus = FlightStatus.Delayed;
        }

        // 6. Calculate new arrival time
        var newArrivalTime = request.DepartureTime.AddMinutes(route.EstimatedDurationMinutes.Value);

        // 7. Update only time fields + status
        var flightDto = new FlightDto
        {
            Id = request.Id,
            DepartureTime = request.DepartureTime,
            ArrivalTime = newArrivalTime,
            Status = (int)newStatus
        };

        await _flightRepository.UpdateAsync(flightDto, request.AirlineId, cancellationToken);
        return Result.Success(true);
    }
}
