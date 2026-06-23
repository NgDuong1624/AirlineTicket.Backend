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

public class RoundTripFlightResult
{
    public List<FlightDto> OutboundFlights { get; set; } = new();
    public List<FlightDto> ReturnFlights { get; set; } = new();
}

public record SearchRoundTripFlightsQuery(
    string OriginCode,
    string DestinationCode,
    DateTime OutboundDate,
    DateTime ReturnDate,
    string? CabinClass = null,
    List<string>? Airlines = null,
    decimal? PriceRangeMin = null,
    decimal? PriceRangeMax = null,
    int? MaxStops = null,
    string? SortBy = null,
    string Currency = "VND") : IQuery<Result<RoundTripFlightResult>>, ICacheableRequest
{
    public string CacheKey => CacheKeyBuilder.ForQuery<SearchRoundTripFlightsQuery>(
        $"{OriginCode}_{DestinationCode}_{OutboundDate:yyyyMMdd}_{ReturnDate:yyyyMMdd}_{CabinClass}_{(Airlines != null ? string.Join("-", Airlines) : "")}_{PriceRangeMin}_{PriceRangeMax}_{MaxStops}_{SortBy}_{Currency}");

    public int CacheDurationMinutes => 5;
}

public class SearchRoundTripFlightsQueryHandler : IQueryHandler<SearchRoundTripFlightsQuery, Result<RoundTripFlightResult>>
{
    private readonly IFlightRepository _flightRepository;

    public SearchRoundTripFlightsQueryHandler(IFlightRepository flightRepository)
    {
        _flightRepository = flightRepository;
    }

    public async Task<Result<RoundTripFlightResult>> Handle(SearchRoundTripFlightsQuery request, CancellationToken cancellationToken)
    {
        // Execute both searches in parallel
        var outboundSearchTask = _flightRepository.SearchAsync(
            request.OriginCode,
            request.DestinationCode,
            request.OutboundDate,
            request.CabinClass,
            request.Airlines,
            request.PriceRangeMin,
            request.PriceRangeMax,
            request.MaxStops,
            request.SortBy,
            request.Currency,
            cancellationToken);

        var returnSearchTask = _flightRepository.SearchAsync(
            request.DestinationCode,
            request.OriginCode,
            request.ReturnDate,
            request.CabinClass,
            request.Airlines,
            request.PriceRangeMin,
            request.PriceRangeMax,
            request.MaxStops,
            request.SortBy,
            request.Currency,
            cancellationToken);

        await Task.WhenAll(outboundSearchTask, returnSearchTask);

        var result = new RoundTripFlightResult
        {
            OutboundFlights = await outboundSearchTask,
            ReturnFlights = await returnSearchTask
        };

        return Result.Success(result);
    }
}
