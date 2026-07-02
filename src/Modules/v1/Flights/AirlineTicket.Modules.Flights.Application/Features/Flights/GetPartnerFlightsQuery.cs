using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record GetPartnerFlightsQuery(Guid AirlineId) : IQuery<Result<List<FlightDto>>>;

internal sealed class GetPartnerFlightsQueryHandler : IQueryHandler<GetPartnerFlightsQuery, Result<List<FlightDto>>>
{
    private readonly IFlightRepository _flightRepository;

    public GetPartnerFlightsQueryHandler(IFlightRepository flightRepository)
    {
        _flightRepository = flightRepository;
    }

    public async Task<Result<List<FlightDto>>> Handle(GetPartnerFlightsQuery request, CancellationToken cancellationToken)
    {
        var flights = await _flightRepository.GetByAirlineAsync(request.AirlineId, cancellationToken);
        return Result.Success(flights);
    }
}
