namespace AirlineTicket.Modules.Flights.Application.Contracts;

public interface IFlightTelemetryCache
{
    Task SetTelemetryAsync(Guid flightId, FlightTelemetryDto telemetry, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
    Task<FlightTelemetryDto?> GetTelemetryAsync(Guid flightId, CancellationToken cancellationToken = default);
    Task SetActiveRadarPlanesAsync(List<AircraftMapPinDto> planes, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
    Task<List<AircraftMapPinDto>?> GetActiveRadarPlanesAsync(CancellationToken cancellationToken = default);
}
