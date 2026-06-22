using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;

namespace AirlineTicket.Modules.Flights.Application.Features.Airports;

public record GetAirportByIdQuery(Guid Id) : IQuery<Result<Airport>>;

internal sealed class GetAirportByIdQueryHandler : IQueryHandler<GetAirportByIdQuery, Result<Airport>>
{
    private readonly IAirportRepository _airportRepository;

    public GetAirportByIdQueryHandler(IAirportRepository airportRepository)
    {
        _airportRepository = airportRepository;
    }

    public async Task<Result<Airport>> Handle(GetAirportByIdQuery request, CancellationToken cancellationToken)
    {
        var airport = await _airportRepository.GetByIdAsync(request.Id, cancellationToken);
        if (airport is null)
        {
            return Result.Failure<Airport>(new Error("Airport.NotFound", "Airport not found"));
        }
        return Result.Success(airport);
    }
}
