using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Domain.Enums;
using MediatR;

namespace AirlineTicket.Modules.Bookings.Application.Features.Bookings;

public record UpdateBookingStatusCommand(Guid BookingId, string Status) : IRequest<Unit>;

public class UpdateBookingStatusCommandHandler : IRequestHandler<UpdateBookingStatusCommand, Unit>
{
    private readonly IBookingRepository _bookingRepository;

    public UpdateBookingStatusCommandHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<Unit> Handle(UpdateBookingStatusCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<BookingStatus>(request.Status, ignoreCase: true, out var status))
            throw new ArgumentException($"Invalid booking status '{request.Status}'. Allowed values: Pending, Confirmed, Cancelled.");

        var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
        if (booking == null)
            throw new KeyNotFoundException($"Booking with ID {request.BookingId} not found.");

        booking.Status = status.ToString();
        await _bookingRepository.UpdateAsync(booking, cancellationToken);
        return Unit.Value;
    }
}
