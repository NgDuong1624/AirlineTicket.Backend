using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Infrastructure.Data;
using AirlineTicket.Modules.Flights.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AirlineTicket.SignalR.Hubs;

/// <summary>
/// Assembles staff ticket-sales rows by joining the Bookings and Flights data stores.
/// Lives in the host so neither module needs a reference to the other.
/// </summary>
public class StaffSalesReader : IStaffSalesReader
{
    private readonly BookingDbContext _bookingContext;
    private readonly FlightDbContext _flightContext;

    public StaffSalesReader(BookingDbContext bookingContext, FlightDbContext flightContext)
    {
        _bookingContext = bookingContext;
        _flightContext = flightContext;
    }

    public async Task<List<StaffSaleDto>> GetSalesAsync(CancellationToken cancellationToken = default)
    {
        // One row per booking, using its first ticket for the flight/seat display.
        var bookings = await _bookingContext.Bookings
            .AsNoTracking()
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new
            {
                b.Id,
                b.PnrCode,
                b.TotalPrice,
                b.Status,
                b.CreatedAt,
                Passenger = b.Passengers.Select(p => p.FirstName + " " + p.LastName).FirstOrDefault(),
                Ticket = b.Tickets
                    .Select(t => new { t.FlightId, t.SeatId })
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var flightIds = bookings.Where(b => b.Ticket != null).Select(b => b.Ticket!.FlightId).Distinct().ToList();
        var seatIds = bookings.Where(b => b.Ticket != null).Select(b => b.Ticket!.SeatId).Distinct().ToList();

        // Resolve flight number + route from the Flights store.
        var flights = await _flightContext.Flights
            .AsNoTracking()
            .Where(f => flightIds.Contains(f.Id))
            .Select(f => new
            {
                f.Id,
                f.FlightNumber,
                Origin = f.Route.OriginAirport.IataCode,
                Destination = f.Route.DestinationAirport.IataCode
            })
            .ToDictionaryAsync(f => f.Id, cancellationToken);

        var seatClasses = await _flightContext.FlightSeats
            .AsNoTracking()
            .Where(s => seatIds.Contains(s.Id))
            .Select(s => new { s.Id, s.SeatClass })
            .ToDictionaryAsync(s => s.Id, s => s.SeatClass, cancellationToken);

        return bookings.Select(b =>
        {
            string flightNumber = string.Empty;
            string route = string.Empty;
            string seatClass = string.Empty;

            if (b.Ticket != null)
            {
                if (flights.TryGetValue(b.Ticket.FlightId, out var f))
                {
                    flightNumber = f.FlightNumber;
                    route = $"{f.Origin} → {f.Destination}";
                }
                if (seatClasses.TryGetValue(b.Ticket.SeatId, out var sc))
                {
                    seatClass = sc.ToString();
                }
            }

            return new StaffSaleDto
            {
                Id = b.PnrCode,
                BookingId = b.Id,
                PassengerName = b.Passenger ?? string.Empty,
                FlightNumber = flightNumber,
                Route = route,
                SeatClass = seatClass,
                Amount = b.TotalPrice,
                Status = b.Status.ToString(),
                BookedAt = b.CreatedAt
            };
        }).ToList();
    }
}
