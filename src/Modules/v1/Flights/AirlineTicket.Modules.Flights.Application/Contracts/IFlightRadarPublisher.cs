namespace AirlineTicket.Modules.Flights.Application.Contracts;

public interface IFlightRadarPublisher
{
    Task PublishTelemetryUpdateAsync(FlightTelemetryDto telemetry, CancellationToken cancellationToken = default);
    Task PublishGlobalRadarTickAsync(List<AircraftMapPinDto> activePlanes, CancellationToken cancellationToken = default);
    Task PublishStatusChangeAsync(FlightStatusChangedDto statusChanged, CancellationToken cancellationToken = default);
}
