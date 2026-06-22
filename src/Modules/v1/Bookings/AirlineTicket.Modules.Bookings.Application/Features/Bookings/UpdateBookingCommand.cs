using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using MediatR;

namespace AirlineTicket.Modules.Bookings.Application.Features.Bookings;

public record UpdateBookingCommand(Guid BookingId, List<PassengerDto>? Passengers, string? ContactEmail, string? ContactPhone) : ICommand<Result<Unit>>;

public class UpdateBookingCommandHandler : ICommandHandler<UpdateBookingCommand, Result<Unit>>
{
    private readonly IBookingRepository _bookingRepository;

    public UpdateBookingCommandHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<Unit>> Handle(UpdateBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
        if (booking == null)
            return Result.Failure<Unit>(new Error("NOT_FOUND", $"Booking with ID {request.BookingId} not found."));

        if (!string.IsNullOrEmpty(request.ContactEmail))
            booking.ContactEmail = request.ContactEmail;

        if (!string.IsNullOrEmpty(request.ContactPhone))
            booking.ContactPhone = request.ContactPhone;

        await _bookingRepository.UpdateAsync(booking, cancellationToken);
        return Result.Success(Unit.Value);
    }
}
