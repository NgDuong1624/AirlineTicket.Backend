using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using MediatR;

namespace AirlineTicket.Modules.Bookings.Application.Features.Payments;

public record PayBookingCommand(Guid BookingId, string PaymentMethod, decimal Amount) : IRequest<Guid>;

public class PayBookingCommandHandler : IRequestHandler<PayBookingCommand, Guid>
{
    private readonly IBookingRepository _bookingRepository;

    public PayBookingCommandHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<Guid> Handle(PayBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
        if (booking == null) throw new Exception("Booking not found");

        booking.Status = "Confirmed";
        await _bookingRepository.UpdateAsync(booking, cancellationToken);
        
        return Guid.NewGuid(); // Transaction Id
    }
}
