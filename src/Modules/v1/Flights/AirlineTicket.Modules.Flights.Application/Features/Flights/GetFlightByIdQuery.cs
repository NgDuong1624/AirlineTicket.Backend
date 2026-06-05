using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record GetFlightByIdQuery(Guid Id) : IQuery<object>;

internal sealed class GetFlightByIdQueryHandler : IQueryHandler<GetFlightByIdQuery, object>
{
    private readonly IFlightRepository _flightRepository;
    public GetFlightByIdQueryHandler(IFlightRepository flightRepository) => _flightRepository = flightRepository;
    public async Task<object> Handle(GetFlightByIdQuery request, CancellationToken cancellationToken)
    {
        var flight = await _flightRepository.GetByIdAsync(request.Id, cancellationToken);
        return flight ?? (object)new { };
    }
}
