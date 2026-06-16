using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Flights.Application.Contracts;
using MediatR;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record GetTrendingFlightsQuery : IRequest<List<FlightDto>>;

public class GetTrendingFlightsQueryHandler : IRequestHandler<GetTrendingFlightsQuery, List<FlightDto>>
{
    private readonly IFlightRepository _flightRepository;

    public GetTrendingFlightsQueryHandler(IFlightRepository flightRepository)
    {
        _flightRepository = flightRepository;
    }

    public async Task<List<FlightDto>> Handle(GetTrendingFlightsQuery request, CancellationToken cancellationToken)
    {
        return await _flightRepository.GetTrendingAsync(cancellationToken);
    }
}