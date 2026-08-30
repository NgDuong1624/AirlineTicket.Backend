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
using AirlineTicket.Modules.Notifications.Application.Features.Commands;
using AirlineTicket.Modules.Users.Application.Repositories;
using AirlineTicket.Modules.Users.Domain.Enums;
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

    public async Task<(List<StaffSaleDto> Items, int TotalCount)> GetSalesAsync(
        int pageIndex,
        int pageSize,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var bookings = await _bookingRepository.GetStaffSalesBookingsAsync(cancellationToken);

        var bookingIdsWithTickets = bookings.Where(b => b.FlightId.HasValue).ToList();
        var flightIds = bookingIdsWithTickets.Select(b => b.FlightId!.Value).Distinct().ToList();
        var seatIds = bookingIdsWithTickets.Where(b => b.SeatId.HasValue).Select(b => b.SeatId!.Value).Distinct().ToList();

        var flights = await _flightRepository.GetByIdsAsync(flightIds, cancellationToken);
        var flightsDict = flights.ToDictionary(f => f.Id);

        var seatClasses = await _flightSeatRepository.GetSeatClassesAsync(seatIds, cancellationToken);

        var allSales = bookings.Select(b =>
        {
            string flightNumber = string.Empty;
            string route = string.Empty;
            string seatClass = string.Empty;
            DateTime? departureAt = null;

            if (b.FlightId.HasValue && flightsDict.TryGetValue(b.FlightId.Value, out var f))
            {
                flightNumber = f.FlightNumber;
                route = $"{f.OriginCode} → {f.DestinationCode}";
                departureAt = f.DepartureTime;
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
                BookedAt = b.CreatedAt,
                DepartureAt = departureAt
            };
        });

        // Apply filtering
        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            allSales = allSales.Where(s => s.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var cleanSearch = search.Trim().ToLower();
            allSales = allSales.Where(s =>
                s.Id.ToLower().Contains(cleanSearch) ||
                s.PassengerName.ToLower().Contains(cleanSearch) ||
                s.FlightNumber.ToLower().Contains(cleanSearch) ||
                s.Route.ToLower().Contains(cleanSearch)
            );
        }

        var filteredList = allSales.ToList();
        var totalCount = filteredList.Count;

        var pagedItems = filteredList
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (pagedItems, totalCount);
    }
}

public class BookingConfirmedEventHandler : INotificationHandler<BookingConfirmedEvent>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IFlightRepository _flightRepository;
    private readonly IMediator _mediator;

    public BookingConfirmedEventHandler(
        IBookingRepository bookingRepository,
        IFlightRepository flightRepository,
        IMediator mediator)
    {
        _bookingRepository = bookingRepository;
        _flightRepository = flightRepository;
        _mediator = mediator;
    }

    public async Task Handle(BookingConfirmedEvent notification, CancellationToken cancellationToken)
    {
        var bookingDetails = await _bookingRepository.GetBookingConfirmedDetailsAsync(notification.BookingId, cancellationToken);

        if (bookingDetails == null || !bookingDetails.FlightId.HasValue) return;

        var flight = await _flightRepository.GetByIdAsync(bookingDetails.FlightId.Value, cancellationToken);
        if (flight == null) return;

        await _mediator.Send(new CreateNotificationCommand(
            UserId: null,
            TemplateCode: "BOOKING_CONFIRMED",
            TemplateParameters: new Dictionary<string, string>
            {
                { "PassengerName", bookingDetails.PassengerName },
                { "PnrCode", bookingDetails.PnrCode },
                { "FlightNumber", flight.FlightNumber }
            },
            Severity: 0,
            ActionUrl: $"/bookings/detail/{bookingDetails.PnrCode}",
            ReferenceId: notification.BookingId,
            ReferenceType: "Booking"
        ), cancellationToken);
    }
}

public class FlightCreatedEventHandler : INotificationHandler<FlightCreatedEvent>
{
    private readonly IUserRepository _userRepository;
    private readonly IRouteRepository _routeRepository;
    private readonly IMediator _mediator;
    private readonly INotificationPusher _notificationPusher;
    private readonly ILogger<FlightCreatedEventHandler> _logger;

    public FlightCreatedEventHandler(
        IUserRepository userRepository,
        IRouteRepository routeRepository,
        IMediator mediator,
        INotificationPusher notificationPusher,
        ILogger<FlightCreatedEventHandler> logger)
    {
        _userRepository = userRepository;
        _routeRepository = routeRepository;
        _mediator = mediator;
        _notificationPusher = notificationPusher;
        _logger = logger;
    }

    public async Task Handle(FlightCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("FlightCreatedEventHandler received FlightCreatedEvent for FlightNumber={FlightNumber}, AirlineId={AirlineId}",
            notification.FlightNumber, notification.AirlineId);

        try
        {
            var route = await _routeRepository.GetByIdAsync(notification.RouteId, cancellationToken);
            var originIata = route?.OriginAirport?.IataCode ?? "N/A";
            var destinationIata = route?.DestinationAirport?.IataCode ?? "N/A";
            var airlineId = notification.AirlineId ?? route?.AirlineId;

            var templateParameters = new Dictionary<string, string>
            {
                { "FlightNumber", notification.FlightNumber },
                { "Origin", originIata },
                { "Destination", destinationIata },
                { "DepartureTime", notification.DepartureTime.ToString("yyyy-MM-dd HH:mm:ss") },
                { "BasePrice", notification.BasePrice.ToString("F2") }
            };

            var targetUsers = new List<AirlineTicket.Modules.Users.Domain.Entities.User>();

            // 1. Fetch Airline Staff & Partner users if airlineId is known
            if (airlineId.HasValue)
            {
                var (staffUsers, _) = await _userRepository.GetAllAsync(
                    search: null,
                    airlineId: airlineId.Value,
                    roleId: null,
                    pageIndex: 1,
                    pageSize: 1000,
                    cancellationToken: cancellationToken);

                targetUsers.AddRange(staffUsers.Where(u => u.Role == (int)UserRole.Staff || u.Role == (int)UserRole.Partner));
            }

            // 2. Fetch System Admins
            var (adminUsers, _) = await _userRepository.GetAllAsync(
                search: null,
                airlineId: null,
                roleId: (int)UserRole.Admin,
                pageIndex: 1,
                pageSize: 1000,
                cancellationToken: cancellationToken);

            targetUsers.AddRange(adminUsers);

            // Deduplicate recipient users
            var distinctUsers = targetUsers
                .GroupBy(u => u.Id)
                .Select(g => g.First())
                .ToList();

            _logger.LogInformation("Creating notifications for {Count} users for flight {FlightNumber}",
                distinctUsers.Count, notification.FlightNumber);

            // 3. Create persistent notification in database per user
            foreach (var user in distinctUsers)
            {
                await _mediator.Send(new CreateNotificationCommand(
                    UserId: user.Id,
                    TemplateCode: "FLIGHT_CREATED",
                    TemplateParameters: templateParameters,
                    Severity: 0,
                    ActionUrl: $"/staff/flights/{notification.FlightId}/seats",
                    ReferenceId: notification.FlightId,
                    ReferenceType: "Flight",
                    Language: user.LanguagePreference ?? "en"
                ), cancellationToken);
            }

            // 4. Real-time push to airline staff group if available
            if (airlineId.HasValue)
            {
                var staffNotificationDto = new AirlineTicket.Modules.Notifications.Application.DTOs.NotificationDto(
                    Guid.NewGuid(),
                    null,
                    $"New Flight Created: {notification.FlightNumber}",
                    $"Flight {notification.FlightNumber} from {originIata} to {destinationIata} departs at {notification.DepartureTime:yyyy-MM-dd HH:mm:ss}.",
                    DateTime.UtcNow);

                await _notificationPusher.PushToAirlineStaffAsync(airlineId.Value, staffNotificationDto, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle FlightCreatedEvent for FlightNumber={FlightNumber}", notification.FlightNumber);
            throw;
        }
    }
}
