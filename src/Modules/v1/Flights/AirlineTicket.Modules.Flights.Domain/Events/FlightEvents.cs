using AirlineTicket.BuildingBlocks.Domain;

namespace AirlineTicket.Modules.Flights.Domain.Events;

public record FlightDepartedDomainEvent(
    Guid FlightId,
    DateTime ActualDepartureTime,
    Guid EventId = default,
    DateTime OccurredOn = default) : IDomainEvent
{
    public Guid EventId { get; init; } = EventId == default ? Guid.NewGuid() : EventId;
    public DateTime OccurredOn { get; init; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

public record FlightLandedDomainEvent(
    Guid FlightId,
    DateTime ActualArrivalTime,
    string? BaggageCarousel,
    Guid EventId = default,
    DateTime OccurredOn = default) : IDomainEvent
{
    public Guid EventId { get; init; } = EventId == default ? Guid.NewGuid() : EventId;
    public DateTime OccurredOn { get; init; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

public record FlightGateChangedDomainEvent(
    Guid FlightId,
    string? OldGate,
    string? NewGate,
    bool IsDeparture,
    Guid EventId = default,
    DateTime OccurredOn = default) : IDomainEvent
{
    public Guid EventId { get; init; } = EventId == default ? Guid.NewGuid() : EventId;
    public DateTime OccurredOn { get; init; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

public record FlightDelayedDomainEvent(
    Guid FlightId,
    int DelayMinutes,
    string? Reason,
    Guid EventId = default,
    DateTime OccurredOn = default) : IDomainEvent
{
    public Guid EventId { get; init; } = EventId == default ? Guid.NewGuid() : EventId;
    public DateTime OccurredOn { get; init; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}
