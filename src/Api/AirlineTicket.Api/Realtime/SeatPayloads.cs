using System;

namespace AirlineTicket.Api.Realtime;

/// <summary>Broadcast to all staff viewing a flight when one seat changes availability.</summary>
public sealed record SeatUpdatedPayload(Guid FlightId, string SeatNumber, bool IsAvailable);
