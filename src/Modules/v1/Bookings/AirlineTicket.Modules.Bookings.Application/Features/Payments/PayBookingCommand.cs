using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Bookings.Application.Contracts;

namespace AirlineTicket.Modules.Bookings.Application.Features.Payments;

public record PayBookingCommand(Guid BookingId, string PaymentMethod, decimal Amount) : ICommand<Result<Guid>>;

public class PayBookingCommandHandler : ICommandHandler<PayBookingCommand, Result<Guid>>
{
    private readonly IBookingRepository _bookingRepository;

    public PayBookingCommandHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<Guid>> Handle(PayBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
        if (booking == null)
            return Result.Failure<Guid>(new Error("NOT_FOUND", "Booking not found"));

        booking.Status = "Confirmed";
        await _bookingRepository.UpdateAsync(booking, cancellationToken);

        return Result.Success(Guid.NewGuid()); // Transaction Id
    }
}
