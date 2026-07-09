using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Flights.Infrastructure.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AirlineTicket.SignalR.Hubs;

/// <summary>
/// Host-side implementation of the Bookings port. Lives here because the API host is the
/// only project that references both the Flights DbContext and SignalR — keeping the
/// Bookings and Flights modules decoupled.
/// </summary>
public class FlightSeatReservation : IFlightSeatReservation
{
    private readonly FlightDbContext _flightContext;
    private readonly IHubContext<SeatHub> _hub;

    public FlightSeatReservation(FlightDbContext flightContext, IHubContext<SeatHub> hub)
    {
        _flightContext = flightContext;
        _hub = hub;
    }

    public async Task<IReadOnlyDictionary<string, ReservedSeat>> ReserveSeatsAsync(
        Guid flightId,
        IReadOnlyCollection<string> seatNumbers,
        CancellationToken cancellationToken = default)
    {
        var wanted = seatNumbers.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        // Read seat metadata (ids/prices/existence) up front for a friendly error + the result.
        var seats = await _flightContext.FlightSeats
            .AsNoTracking()
            .Where(fs => fs.FlightId == flightId && wanted.Contains(fs.SeatNumber))
            .ToListAsync(cancellationToken);

        var found = seats.Select(s => s.SeatNumber).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missing = wanted.Where(w => !found.Contains(w)).ToList();
        if (missing.Count > 0)
            throw new SeatUnavailableException($"Seat(s) not found on this flight: {string.Join(", ", missing)}.");

        var basePrice = await _flightContext.Flights
            .Where(f => f.Id == flightId)
            .Select(f => (decimal?)f.BasePrice)
            .FirstOrDefaultAsync(cancellationToken) ?? 0m;

        // Atomic compare-and-set per seat: the UPDATE only matches rows still IsAvailable,
        // so concurrent requests for the same seat are serialized by row locks and exactly
        // one wins (affected == 1). This prevents double-booking without a rowversion column.
        var won = new List<string>();
        var conflicts = new List<string>();
        foreach (var seatNumber in wanted)
        {
            var sn = seatNumber;
            var affected = await _flightContext.FlightSeats
                .Where(fs => fs.FlightId == flightId && fs.SeatNumber == sn && fs.IsAvailable)
                .ExecuteUpdateAsync(s => s.SetProperty(fs => fs.IsAvailable, false), cancellationToken);

            if (affected == 1) won.Add(sn);
            else conflicts.Add(sn);
        }

        if (conflicts.Count > 0)
        {
            // Roll back only the seats this request actually claimed.
            if (won.Count > 0)
            {
                await _flightContext.FlightSeats
                    .Where(fs => fs.FlightId == flightId && won.Contains(fs.SeatNumber))
                    .ExecuteUpdateAsync(s => s.SetProperty(fs => fs.IsAvailable, true), cancellationToken);
            }
            throw new SeatUnavailableException($"Seat(s) already booked: {string.Join(", ", conflicts)}.");
        }

        var result = new Dictionary<string, ReservedSeat>(StringComparer.OrdinalIgnoreCase);
        foreach (var seat in seats)
            result[seat.SeatNumber] = new ReservedSeat(seat.Id, seat.PriceOverride ?? basePrice);

        await BroadcastAsync(flightId, wanted, isAvailable: false, cancellationToken);
        return result;
    }

    public async Task ReleaseSeatsAsync(
        Guid flightId,
        IReadOnlyCollection<string> seatNumbers,
        CancellationToken cancellationToken = default)
    {
        var wanted = seatNumbers.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        await _flightContext.FlightSeats
            .Where(fs => fs.FlightId == flightId && wanted.Contains(fs.SeatNumber))
            .ExecuteUpdateAsync(s => s.SetProperty(fs => fs.IsAvailable, true), cancellationToken);

        await BroadcastAsync(flightId, wanted, isAvailable: true, cancellationToken);
    }

    private async Task BroadcastAsync(Guid flightId, IEnumerable<string> seatNumbers, bool isAvailable, CancellationToken ct)
    {
        foreach (var seatNumber in seatNumbers)
        {
            await _hub.Clients.Group(SeatHub.GroupFor(flightId))
                .SendAsync("SeatUpdated", new SeatUpdatedPayload(flightId, seatNumber, isAvailable), ct);
        }
    }
}
