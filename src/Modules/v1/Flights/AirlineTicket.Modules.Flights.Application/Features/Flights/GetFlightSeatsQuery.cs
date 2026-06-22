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

    public GetFlightSeatsQueryHandler(IFlightSeatRepository flightSeatRepository)
    {
        _flightSeatRepository = flightSeatRepository;
    }

    public async Task<Result<List<FlightSeat>>> Handle(GetFlightSeatsQuery request, CancellationToken cancellationToken)
    {
        var seats = await _flightSeatRepository.GetByFlightIdAsync(request.FlightId, cancellationToken);
        return Result.Success(seats);
    }
}
