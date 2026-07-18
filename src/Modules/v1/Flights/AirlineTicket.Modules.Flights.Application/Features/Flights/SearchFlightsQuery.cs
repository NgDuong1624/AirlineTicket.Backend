using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.BuildingBlocks.Behaviors;
using AirlineTicket.BuildingBlocks.Caching;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record SearchFlightsQuery(
    string OriginCode,
    string DestinationCode,
    DateTime Date,
    string? CabinClass = null,
    List<string>? Airlines = null,
    decimal? PriceRangeMin = null,
    decimal? PriceRangeMax = null,
    int? MaxStops = null,
    string? SortBy = null,
    string Currency = "VND",
    int PageIndex = 1,
    int PageSize = 10) : IQuery<Result<PagedResult<FlightDto>>>, ICacheableRequest
{
    public string CacheKey => CacheKeyBuilder.ForQuery<SearchFlightsQuery>(
        $"{OriginCode}_{DestinationCode}_{Date:yyyyMMdd}_{CabinClass}_{(Airlines != null ? string.Join("-", Airlines) : "")}_{PriceRangeMin}_{PriceRangeMax}_{MaxStops}_{SortBy}_{Currency}_{PageIndex}_{PageSize}");

    public int CacheDurationMinutes => 5;
}

public class SearchFlightsQueryHandler : IQueryHandler<SearchFlightsQuery, Result<PagedResult<FlightDto>>>
{
    private readonly IFlightRepository _flightRepository;

    public SearchFlightsQueryHandler(IFlightRepository flightRepository)
    {
        _flightRepository = flightRepository;
    }

    public async Task<Result<PagedResult<FlightDto>>> Handle(SearchFlightsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _flightRepository.SearchAsync(
            request.OriginCode,
            request.DestinationCode,
            request.Date,
            request.CabinClass,
            request.Airlines,
            request.PriceRangeMin,
            request.PriceRangeMax,
            request.MaxStops,
            request.SortBy,
            request.Currency,
            request.PageIndex,
            request.PageSize,
            cancellationToken);
        return Result.Success(PagedResult<FlightDto>.Success(items, request.PageIndex, request.PageSize, totalCount));
    }
}
