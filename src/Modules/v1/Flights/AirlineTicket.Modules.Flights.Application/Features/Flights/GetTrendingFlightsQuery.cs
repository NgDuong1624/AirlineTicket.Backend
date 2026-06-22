using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.BuildingBlocks.Behaviors;
using AirlineTicket.BuildingBlocks.Caching;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record GetTrendingFlightsQuery : IQuery<Result<List<FlightDto>>>, ICacheableRequest
{
    public string CacheKey => CacheKeyBuilder.ForQuery<GetTrendingFlightsQuery>("all");
    public int CacheDurationMinutes => 10;
}

public class GetTrendingFlightsQueryHandler : IQueryHandler<GetTrendingFlightsQuery, Result<List<FlightDto>>>
{
    private readonly IFlightRepository _flightRepository;

    public GetTrendingFlightsQueryHandler(IFlightRepository flightRepository)
    {
        _flightRepository = flightRepository;
    }

    public async Task<Result<List<FlightDto>>> Handle(GetTrendingFlightsQuery request, CancellationToken cancellationToken)
    {
        var flights = await _flightRepository.GetTrendingAsync(cancellationToken);
        return Result.Success(flights);
    }
}
