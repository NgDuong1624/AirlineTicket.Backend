using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.Application.Events;
using AirlineTicket.Modules.Notifications.Application.Contracts;
using AirlineTicket.Modules.Notifications.Domain.Entities;
using AirlineTicket.Modules.Bookings.Infrastructure.Data;
using AirlineTicket.Modules.Flights.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirlineTicket.Api.EventHandlers;

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
