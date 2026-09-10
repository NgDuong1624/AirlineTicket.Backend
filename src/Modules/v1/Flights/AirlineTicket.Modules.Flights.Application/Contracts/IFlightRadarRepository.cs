using AirlineTicket.Modules.Flights.Domain.Enums;

namespace AirlineTicket.Modules.Flights.Application.Contracts;

public interface IFlightRadarRepository
{
    Task<List<AircraftMapPinDto>> GetActiveAirborneFlightsAsync(CancellationToken cancellationToken = default);
    Task<FlightTelemetryDto?> GetFlightTelemetryAsync(Guid flightId, CancellationToken cancellationToken = default);
    Task<FlightStatusDetailDto?> GetFlightStatusByNumberAsync(string flightNumber, DateTime? date = null, CancellationToken cancellationToken = default);
    Task<FlightStatusDetailDto?> UpdateFlightStatusAndGateAsync(
        Guid flightId,
        FlightStatus? status,
        string? departureGate,
        string? arrivalGate,
        string? baggageCarousel,
        int? delayMinutes,
        string? reason,
        CancellationToken cancellationToken = default);
}
