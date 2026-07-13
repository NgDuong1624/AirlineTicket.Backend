using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.Application.Events;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Notifications.Application.Contracts;
using AirlineTicket.Modules.Notifications.Domain.Entities;
using MediatR;

namespace AirlineTicket.Api.Services;

/// <summary>
/// Host-side implementation of the Bookings port. Lives here because the API host
/// references both the Bookings and Flights DbContext — keeping the two modules decoupled.
/// This version operates without direct SignalR broadcasting (the dedicated SignalR host
/// handles real-time seat updates).
/// </summary>
public class FlightSeatReservation : IFlightSeatReservation
{
    private readonly IFlightSeatRepository _flightSeatRepository;
    private readonly IFlightRepository _flightRepository;

    public FlightSeatReservation(IFlightSeatRepository flightSeatRepository, IFlightRepository flightRepository)
    {
        _flightSeatRepository = flightSeatRepository;
        _flightRepository = flightRepository;
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

        return result;
    }

    public async Task ReleaseSeatsAsync(
        Guid flightId,
        IReadOnlyCollection<string> seatNumbers,
        CancellationToken cancellationToken = default)
    {
        var wanted = seatNumbers.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        await _flightSeatRepository.ReleaseSeatsAsync(flightId, wanted, cancellationToken);
    }
}

/// <summary>
/// Assembles staff ticket-sales rows by joining the Bookings and Flights data stores.
/// Lives in the host so neither module needs a reference to the other.
/// </summary>
public class StaffSalesReader : IStaffSalesReader
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IFlightRepository _flightRepository;
    private readonly IFlightSeatRepository _flightSeatRepository;

    public StaffSalesReader(
        IBookingRepository bookingRepository,
        IFlightRepository flightRepository,
        IFlightSeatRepository flightSeatRepository)
    {
        _bookingRepository = bookingRepository;
        _flightRepository = flightRepository;
        _flightSeatRepository = flightSeatRepository;
    }

    public async Task<List<StaffSaleDto>> GetSalesAsync(CancellationToken cancellationToken = default)
    {
        var bookings = await _bookingRepository.GetStaffSalesBookingsAsync(cancellationToken);

        var bookingIdsWithTickets = bookings.Where(b => b.FlightId.HasValue).ToList();
        var flightIds = bookingIdsWithTickets.Select(b => b.FlightId!.Value).Distinct().ToList();
        var seatIds = bookingIdsWithTickets.Where(b => b.SeatId.HasValue).Select(b => b.SeatId!.Value).Distinct().ToList();

        var flights = await _flightRepository.GetByIdsAsync(flightIds, cancellationToken);
        var flightsDict = flights.ToDictionary(f => f.Id);

        var seatClasses = await _flightSeatRepository.GetSeatClassesAsync(seatIds, cancellationToken);

        return bookings.Select(b =>
        {
            string flightNumber = string.Empty;
            string route = string.Empty;
            string seatClass = string.Empty;

            if (b.FlightId.HasValue && flightsDict.TryGetValue(b.FlightId.Value, out var f))
            {
                flightNumber = f.FlightNumber;
                route = $"{f.OriginCode} → {f.DestinationCode}";
            }
            if (b.SeatId.HasValue && seatClasses.TryGetValue(b.SeatId.Value, out var sc))
            {
                seatClass = sc;
            }

            return new StaffSaleDto
            {
                Id = b.PnrCode,
                BookingId = b.Id,
                PassengerName = b.PassengerName,
                FlightNumber = flightNumber,
                Route = route,
                SeatClass = seatClass,
                Amount = b.TotalPrice,
                Status = b.Status,
                BookedAt = b.CreatedAt
            };
        }).ToList();
    }
}

public class BookingConfirmedEventHandler : INotificationHandler<BookingConfirmedEvent>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IFlightRepository _flightRepository;
    private readonly INotificationRepository _notificationRepository;

    public BookingConfirmedEventHandler(
        IBookingRepository bookingRepository,
        IFlightRepository flightRepository,
        INotificationRepository notificationRepository)
    {
        _bookingRepository = bookingRepository;
        _flightRepository = flightRepository;
        _notificationRepository = notificationRepository;
    }

    public async Task Handle(BookingConfirmedEvent notification, CancellationToken cancellationToken)
    {
        var bookingDetails = await _bookingRepository.GetBookingConfirmedDetailsAsync(notification.BookingId, cancellationToken);

        if (bookingDetails == null || !bookingDetails.FlightId.HasValue) return;

        var flight = await _flightRepository.GetByIdAsync(bookingDetails.FlightId.Value, cancellationToken);
        if (flight == null) return;

        var emailContent = $"Booking confirmed! View details: {notification.Origin}/bookings/detail/{bookingDetails.PnrCode}";

        var notificationEntity = new Notification
        {
            Type = 0, // Email
            Status = 0, // Pending
            Recipient = bookingDetails.ContactEmail,
            Content = emailContent
        };

        await _notificationRepository.AddAsync(notificationEntity);
    }
}
