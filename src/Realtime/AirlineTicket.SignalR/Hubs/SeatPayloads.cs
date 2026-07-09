using System;

namespace AirlineTicket.SignalR.Hubs;

/// <summary>Broadcast to all staff viewing a flight when one seat changes availability.</summary>
public sealed record SeatUpdatedPayload(Guid FlightId, string SeatNumber, bool IsAvailable);
