using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.Application.Events;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using MediatR;

namespace AirlineTicket.Modules.Bookings.Application.Features.Payments;

public record PayBookingCommand(Guid BookingId, string PaymentMethod, decimal Amount, string Origin) : ICommand<Result<Guid>>;

public class PayBookingCommandHandler : ICommandHandler<PayBookingCommand, Result<Guid>>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IPublisher _publisher;

    public PayBookingCommandHandler(IBookingRepository bookingRepository, IPublisher publisher)
    {
        _bookingRepository = bookingRepository;
        _publisher = publisher;
    }

    public async Task<Result<Guid>> Handle(PayBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
        if (booking == null)
            return Result.Failure<Guid>(new Error("Booking.NotFound", "Booking not found"));

        booking.Status = "Confirmed";
        await _bookingRepository.UpdateAsync(booking, cancellationToken);

        await _publisher.Publish(new BookingConfirmedEvent(request.BookingId, request.Origin), cancellationToken);

        return Result.Success(Guid.NewGuid()); // Transaction Id
    }
}
