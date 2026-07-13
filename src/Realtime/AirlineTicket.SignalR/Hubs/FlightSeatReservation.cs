using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Flights.Application.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace AirlineTicket.SignalR.Hubs;

/// <summary>
/// Host-side implementation of the Bookings port. Lives here because the API host is the
/// only project that references both the Flights DbContext and SignalR — keeping the
/// Bookings and Flights modules decoupled.
/// </summary>
public class FlightSeatReservation : IFlightSeatReservation
{
    private readonly IFlightSeatRepository _flightSeatRepository;
    private readonly IFlightRepository _flightRepository;
    private readonly IHubContext<SeatHub> _hub;

    public FlightSeatReservation(IFlightSeatRepository flightSeatRepository, IFlightRepository flightRepository, IHubContext<SeatHub> hub)
    {
        _flightSeatRepository = flightSeatRepository;
        _flightRepository = flightRepository;
        _hub = hub;
    }

    public async Task<IReadOnlyDictionary<string, ReservedSeat>> ReserveSeatsAsync(
        Guid flightId,
        IReadOnlyCollection<string> seatNumbers,
        CancellationToken cancellationToken = default)
    {
        var wanted = seatNumbers.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        var seats = await _flightSeatRepository.GetSeatsByNumbersAsync(flightId, wanted, cancellationToken);

        var found = seats.Select(s => s.SeatNumber).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missing = wanted.Where(w => !found.Contains(w)).ToList();
        if (missing.Count > 0)
            throw new SeatUnavailableException($"Seat(s) not found on this flight: {string.Join(", ", missing)}.");

        var basePrice = await _flightSeatRepository.GetFlightBasePriceAsync(flightId, cancellationToken);

        // Atomic compare-and-set per seat
        var won = new List<string>();
        var conflicts = new List<string>();
        foreach (var seatNumber in wanted)
        {
            var sn = seatNumber;
            var affected = await _flightSeatRepository.ReserveSeatAsync(flightId, sn, cancellationToken);

            if (affected == 1) won.Add(sn);
            else conflicts.Add(sn);
        }

        if (conflicts.Count > 0)
        {
            if (won.Count > 0)
            {
                await _flightSeatRepository.ReleaseSeatsAsync(flightId, won, cancellationToken);
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

        await _flightSeatRepository.ReleaseSeatsAsync(flightId, wanted, cancellationToken);

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
