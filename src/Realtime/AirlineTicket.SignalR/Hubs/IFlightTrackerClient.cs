using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.SignalR.Hubs;

public interface IFlightTrackerClient
{
    Task TelemetryUpdated(FlightTelemetryDto telemetry);
    Task GlobalRadarTick(List<AircraftMapPinDto> activePlanes);
    Task FlightStatusChanged(FlightStatusChangedDto statusUpdate);
}
