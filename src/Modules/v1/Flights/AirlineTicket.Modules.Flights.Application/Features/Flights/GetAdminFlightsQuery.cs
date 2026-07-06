using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record GetAdminFlightsQuery() : IQuery<Result<List<FlightDto>>>;

internal sealed class GetAdminFlightsQueryHandler : IQueryHandler<GetAdminFlightsQuery, Result<List<FlightDto>>>
{
    private readonly IFlightRepository _flightRepository;

    public GetAdminFlightsQueryHandler(IFlightRepository flightRepository)
    {
        _flightRepository = flightRepository;
    }

    public async Task<Result<List<FlightDto>>> Handle(GetAdminFlightsQuery request, CancellationToken cancellationToken)
    {
        var flights = await _flightRepository.GetAllAsync(cancellationToken);
        return Result.Success(flights);
    }
}
