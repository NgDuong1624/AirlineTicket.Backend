using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.Modules.Flights.Application.Features.Airports;

public record GetAirportsQuery(string? Search) : IQuery<object>;

internal sealed class GetAirportsQueryHandler : IQueryHandler<GetAirportsQuery, object>
{
    private readonly IAirportRepository _airportRepository;
    public GetAirportsQueryHandler(IAirportRepository airportRepository) => _airportRepository = airportRepository;

    public async Task<object> Handle(GetAirportsQuery request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.Search))
            return await _airportRepository.SearchAsync(request.Search, cancellationToken);
        return await _airportRepository.GetAllAsync(cancellationToken);
    }
}
