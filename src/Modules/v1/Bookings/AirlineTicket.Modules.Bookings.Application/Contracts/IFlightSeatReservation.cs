using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Bookings.Application.Contracts;

/// <summary>
/// Port the Bookings module uses to reserve seats that physically live in the Flights
/// module. Implemented in the API host (the only project that references both DbContexts),
/// keeping the module boundary intact.
/// </summary>
public interface IFlightSeatReservation
{
    /// <summary>
    /// Atomically marks the given seats unavailable for a flight and returns the resolved
    /// seat ids + price keyed by seat number. Throws <see cref="SeatUnavailableException"/>
    /// if any seat does not exist or is already taken.
    /// </summary>
    Task<IReadOnlyDictionary<string, ReservedSeat>> ReserveSeatsAsync(
        Guid flightId,
        IReadOnlyCollection<string> seatNumbers,
        CancellationToken cancellationToken = default);

    /// <summary>Releases seats back to available (used when a booking is cancelled).</summary>
    Task ReleaseSeatsAsync(
        Guid flightId,
        IReadOnlyCollection<string> seatNumbers,
        CancellationToken cancellationToken = default);
}

public sealed record ReservedSeat(Guid SeatId, decimal Price);

public sealed class SeatUnavailableException : Exception
{
    public SeatUnavailableException(string message) : base(message) { }
}
