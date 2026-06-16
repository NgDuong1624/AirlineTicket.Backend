using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Flights.Application.Contracts;
using MediatR;

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
    string Currency = "VND") : IRequest<List<FlightDto>>;

public class SearchFlightsQueryHandler : IRequestHandler<SearchFlightsQuery, List<FlightDto>>
{
    private readonly IFlightRepository _flightRepository;

    public SearchFlightsQueryHandler(IFlightRepository flightRepository)
    {
        _flightRepository = flightRepository;
    }

    public async Task<List<FlightDto>> Handle(SearchFlightsQuery request, CancellationToken cancellationToken)
    {
        return await _flightRepository.SearchAsync(
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
            cancellationToken);
    }
}