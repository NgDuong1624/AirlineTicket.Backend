using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.Application.Events;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Infrastructure.Data;
using AirlineTicket.Modules.Flights.Infrastructure.Data;
using AirlineTicket.Modules.Notifications.Application.Contracts;
using AirlineTicket.Modules.Notifications.Domain.Entities;
using MediatR;
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

public class BookingConfirmedEventHandler : INotificationHandler<BookingConfirmedEvent>
{
    private readonly BookingDbContext _bookingDbContext;
    private readonly FlightDbContext _flightDbContext;
    private readonly INotificationRepository _notificationRepository;

    public BookingConfirmedEventHandler(
        BookingDbContext bookingDbContext,
        FlightDbContext flightDbContext,
        INotificationRepository notificationRepository)
    {
        _bookingDbContext = bookingDbContext;
        _flightDbContext = flightDbContext;
        _notificationRepository = notificationRepository;
    }

    public async Task Handle(BookingConfirmedEvent notification, CancellationToken cancellationToken)
    {
        var booking = await _bookingDbContext.Bookings
            .Include(b => b.Passengers)
            .Include(b => b.Tickets)
            .FirstOrDefaultAsync(b => b.Id == notification.BookingId, cancellationToken);

        if (booking == null) return;

        var ticket = booking.Tickets.FirstOrDefault();
        if (ticket == null) return;

        var flight = await _flightDbContext.Flights
            .Include(f => f.Route)
            .ThenInclude(r => r.OriginAirport)
            .Include(f => f.Route)
            .ThenInclude(r => r.DestinationAirport)
            .FirstOrDefaultAsync(f => f.Id == ticket.FlightId, cancellationToken);

        if (flight == null) return;

        var emailContent = $"Booking confirmed! View details: {notification.Origin}/bookings/detail/{booking.PnrCode}";

        var notificationEntity = new Notification
        {
            Type = 0, // Email
            Status = 0, // Pending
            Recipient = booking.ContactEmail,
            Content = emailContent
        };

        await _notificationRepository.AddAsync(notificationEntity);
    }
}
