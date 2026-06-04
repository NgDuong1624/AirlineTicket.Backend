using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Flights.Application.Contracts;
using MediatR;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record SearchFlightsQuery(string OriginCode, string DestinationCode, DateTime Date) : IRequest<object>;

public class SearchFlightsQueryHandler : IRequestHandler<SearchFlightsQuery, object>
{
    private readonly IFlightRepository _flightRepository;
    
    public SearchFlightsQueryHandler(IFlightRepository flightRepository)
    {
        _flightRepository = flightRepository;
    }
    
    public async Task<object> Handle(SearchFlightsQuery request, CancellationToken cancellationToken)
    {
        return await _flightRepository.SearchAsync(request.OriginCode, request.DestinationCode, request.Date, cancellationToken);
    }
}
