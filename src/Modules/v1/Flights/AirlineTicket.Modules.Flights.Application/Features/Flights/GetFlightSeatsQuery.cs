using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record GetFlightSeatsQuery(Guid FlightId) : IQuery<object>;

internal sealed class GetFlightSeatsQueryHandler : IQueryHandler<GetFlightSeatsQuery, object>
{
    private readonly IFlightSeatRepository _flightSeatRepository;
    public GetFlightSeatsQueryHandler(IFlightSeatRepository flightSeatRepository) => _flightSeatRepository = flightSeatRepository;
    public async Task<object> Handle(GetFlightSeatsQuery request, CancellationToken cancellationToken) => await _flightSeatRepository.GetByFlightIdAsync(request.FlightId, cancellationToken);
}
