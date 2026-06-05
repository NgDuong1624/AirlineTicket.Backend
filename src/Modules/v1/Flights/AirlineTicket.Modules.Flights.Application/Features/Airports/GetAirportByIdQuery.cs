using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.Modules.Flights.Application.Features.Airports;

public record GetAirportByIdQuery(Guid Id) : IQuery<object>;

internal sealed class GetAirportByIdQueryHandler : IQueryHandler<GetAirportByIdQuery, object>
{
    private readonly IAirportRepository _airportRepository;
    public GetAirportByIdQueryHandler(IAirportRepository airportRepository) => _airportRepository = airportRepository;
    public async Task<object> Handle(GetAirportByIdQuery request, CancellationToken cancellationToken)
    {
        var airport = await _airportRepository.GetByIdAsync(request.Id, cancellationToken);
        return airport ?? (object)new { };
    }
}
