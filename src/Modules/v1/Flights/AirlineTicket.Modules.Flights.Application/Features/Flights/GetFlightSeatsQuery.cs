using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record GetFlightSeatsQuery(Guid FlightId) : IQuery<Result<List<FlightSeat>>>;

internal sealed class GetFlightSeatsQueryHandler : IQueryHandler<GetFlightSeatsQuery, Result<List<FlightSeat>>>
{
    private readonly IFlightSeatRepository _flightSeatRepository;
    private readonly IFlightRepository _flightRepository;

    public GetFlightSeatsQueryHandler(IFlightSeatRepository flightSeatRepository, IFlightRepository flightRepository)
    {
        _flightSeatRepository = flightSeatRepository;
        _flightRepository = flightRepository;
    }

    public async Task<Result<List<FlightSeat>>> Handle(GetFlightSeatsQuery request, CancellationToken cancellationToken)
    {
        var flight = await _flightRepository.GetByIdAsync(request.FlightId, cancellationToken);
        if (flight is null)
        {
            return Result.Failure<List<FlightSeat>>(new Error("Flight.NotFound", "Flight not found."));
        }

        var seats = await _flightSeatRepository.GetByFlightIdAsync(request.FlightId, cancellationToken);
        return Result.Success(seats);
    }
}
