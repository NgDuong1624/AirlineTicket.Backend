using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.Modules.Flights.Application.Features.Radar;

public record GetLiveFlightTelemetryQuery(Guid FlightId) : IQuery<Result<FlightTelemetryDto>>;

public sealed class GetLiveFlightTelemetryQueryHandler : IQueryHandler<GetLiveFlightTelemetryQuery, Result<FlightTelemetryDto>>
{
    private readonly IFlightTelemetryCache _cache;
    private readonly IFlightRadarRepository _repository;

    public GetLiveFlightTelemetryQueryHandler(
        IFlightTelemetryCache cache,
        IFlightRadarRepository repository)
    {
        _cache = cache;
        _repository = repository;
    }

    public async Task<Result<FlightTelemetryDto>> Handle(GetLiveFlightTelemetryQuery request, CancellationToken cancellationToken)
    {
        var cached = await _cache.GetTelemetryAsync(request.FlightId, cancellationToken);
        if (cached != null)
        {
            return Result.Success(cached);
        }

        var telemetry = await _repository.GetFlightTelemetryAsync(request.FlightId, cancellationToken);
        if (telemetry == null)
        {
            return Result.Failure<FlightTelemetryDto>(new Error("Flight.NotFound", $"Flight with ID {request.FlightId} was not found."));
        }

        await _cache.SetTelemetryAsync(request.FlightId, telemetry, TimeSpan.FromMinutes(5), cancellationToken);

        return Result.Success(telemetry);
    }
}
