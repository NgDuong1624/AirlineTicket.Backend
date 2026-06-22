using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.BuildingBlocks.Behaviors;
using AirlineTicket.BuildingBlocks.Caching;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record GetFlightByIdQuery(Guid Id) : IQuery<Result<FlightDto>>, ICacheableRequest
{
    public string CacheKey => CacheKeyBuilder.ForQuery<GetFlightByIdQuery>(Id.ToString());
    public int CacheDurationMinutes => 5;
}

public class GetFlightByIdQueryHandler : IQueryHandler<GetFlightByIdQuery, Result<FlightDto>>
{
    private readonly IFlightRepository _flightRepository;

    public GetFlightByIdQueryHandler(IFlightRepository flightRepository)
    {
        _flightRepository = flightRepository;
    }

    public async Task<Result<FlightDto>> Handle(GetFlightByIdQuery request, CancellationToken cancellationToken)
    {
        var flight = await _flightRepository.GetByIdAsync(request.Id, cancellationToken);
        if (flight is null)
        {
            return Result.Failure<FlightDto>(new Error("Flight.NotFound", "Flight not found"));
        }
        return Result.Success(flight);
    }
}
