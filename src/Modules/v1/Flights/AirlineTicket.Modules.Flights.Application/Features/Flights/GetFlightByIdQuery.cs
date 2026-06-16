using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Flights.Application.Contracts;
using MediatR;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record GetFlightByIdQuery(Guid Id) : IRequest<FlightDto?>;

public class GetFlightByIdQueryHandler : IRequestHandler<GetFlightByIdQuery, FlightDto?>
{
    private readonly IFlightRepository _flightRepository;

    public GetFlightByIdQueryHandler(IFlightRepository flightRepository)
    {
        _flightRepository = flightRepository;
    }

    public async Task<FlightDto?> Handle(GetFlightByIdQuery request, CancellationToken cancellationToken)
    {
        return await _flightRepository.GetByIdAsync(request.Id, cancellationToken);
    }
}