using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.BuildingBlocks.Behaviors;
using AirlineTicket.BuildingBlocks.Caching;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record GetTrendingFlightsQuery(int PageIndex = 1, int PageSize = 5) : IQuery<Result<PagedResult<FlightDto>>>, ICacheableRequest
{
    public string CacheKey => CacheKeyBuilder.ForQuery<GetTrendingFlightsQuery>("all");
    public int CacheDurationMinutes => 10;
}

public class GetTrendingFlightsQueryHandler : IQueryHandler<GetTrendingFlightsQuery, Result<PagedResult<FlightDto>>>
{
    private readonly IFlightRepository _flightRepository;

    public GetTrendingFlightsQueryHandler(IFlightRepository flightRepository)
    {
        _flightRepository = flightRepository;
    }

    public async Task<Result<PagedResult<FlightDto>>> Handle(GetTrendingFlightsQuery request, CancellationToken cancellationToken)
    {
        var flights = await _flightRepository.GetTrendingAsync(cancellationToken);
        var totalCount = flights.Count;
        var items = flights
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();
        return Result.Success(PagedResult<FlightDto>.Success(items, request.PageIndex, request.PageSize, totalCount));
    }
}
