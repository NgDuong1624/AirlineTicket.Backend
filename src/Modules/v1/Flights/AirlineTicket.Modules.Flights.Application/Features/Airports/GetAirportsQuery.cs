using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.BuildingBlocks.Behaviors;
using AirlineTicket.BuildingBlocks.Caching;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;

namespace AirlineTicket.Modules.Flights.Application.Features.Airports;

public record GetAirportsQuery(string? Search) : IQuery<Result<List<Airport>>>, ICacheableRequest
{
    public string CacheKey => CacheKeyBuilder.ForQuery<GetAirportsQuery>(Search ?? "all");
    public int CacheDurationMinutes => 60; // Airports rarely change
}

internal sealed class GetAirportsQueryHandler : IQueryHandler<GetAirportsQuery, Result<List<Airport>>>
{
    private readonly IAirportRepository _airportRepository;

    public GetAirportsQueryHandler(IAirportRepository airportRepository)
    {
        _airportRepository = airportRepository;
    }

    public async Task<Result<List<Airport>>> Handle(GetAirportsQuery request, CancellationToken cancellationToken)
    {
        List<Airport> airports;
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            airports = await _airportRepository.SearchAsync(request.Search, cancellationToken);
        }
        else
        {
            airports = await _airportRepository.GetAllAsync(cancellationToken);
        }
        return Result.Success(airports);
    }
}
