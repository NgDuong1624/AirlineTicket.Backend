using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.Modules.Flights.Application.Features.Radar;

public record GetActiveAirborneFlightsQuery : IQuery<Result<List<AircraftMapPinDto>>>;

public sealed class GetActiveAirborneFlightsQueryHandler : IQueryHandler<GetActiveAirborneFlightsQuery, Result<List<AircraftMapPinDto>>>
{
    private readonly IFlightTelemetryCache _cache;
    private readonly IFlightRadarRepository _repository;

    public GetActiveAirborneFlightsQueryHandler(
        IFlightTelemetryCache cache,
        IFlightRadarRepository repository)
    {
        _cache = cache;
        _repository = repository;
    }

    public async Task<Result<List<AircraftMapPinDto>>> Handle(GetActiveAirborneFlightsQuery request, CancellationToken cancellationToken)
    {
        var cached = await _cache.GetActiveRadarPlanesAsync(cancellationToken);
        if (cached != null && cached.Count > 0)
        {
            return Result.Success(cached);
        }

        var pins = await _repository.GetActiveAirborneFlightsAsync(cancellationToken);
        await _cache.SetActiveRadarPlanesAsync(pins, TimeSpan.FromSeconds(15), cancellationToken);

        return Result.Success(pins);
    }
}
