using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Flights.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AirlineTicket.Api.Services;

/// <summary>
/// Host-side implementation of the Bookings port. Lives here because the API host
/// references both the Bookings and Flights DbContext — keeping the two modules decoupled.
/// This version operates without direct SignalR broadcasting (the dedicated SignalR host
/// handles real-time seat updates).
/// </summary>
public class FlightSeatReservation : IFlightSeatReservation
{
    private readonly FlightDbContext _flightContext;

    public FlightSeatReservation(FlightDbContext flightContext)
    {
        _flightContext = flightContext;
    }

    public async Task<IReadOnlyDictionary<string, ReservedSeat>> ReserveSeatsAsync(
        Guid flightId,
        IReadOnlyCollection<string> seatNumbers,
        CancellationToken cancellationToken = default)
    {
        var wanted = seatNumbers.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

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

        // Atomic compare-and-set per seat
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
    }
}
