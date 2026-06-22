using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Domain.Enums;
using MediatR;

namespace AirlineTicket.Modules.Bookings.Application.Features.Bookings;

public record UpdateBookingStatusCommand(Guid BookingId, string Status) : ICommand<Result<Unit>>;

public class UpdateBookingStatusCommandHandler : ICommandHandler<UpdateBookingStatusCommand, Result<Unit>>
{
    private readonly IBookingRepository _bookingRepository;

    public UpdateBookingStatusCommandHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<Unit>> Handle(UpdateBookingStatusCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<BookingStatus>(request.Status, ignoreCase: true, out var status))
            return Result.Failure<Unit>(new Error("BAD_REQUEST", $"Invalid booking status '{request.Status}'. Allowed values: Pending, Confirmed, Cancelled."));

        var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
        if (booking == null)
            return Result.Failure<Unit>(new Error("NOT_FOUND", $"Booking with ID {request.BookingId} not found."));

        booking.Status = status.ToString();
        await _bookingRepository.UpdateAsync(booking, cancellationToken);
        return Result.Success(Unit.Value);
    }
}
