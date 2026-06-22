using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using MediatR;

namespace AirlineTicket.Modules.Bookings.Application.Features.Bookings;

public record CancelBookingCommand(Guid BookingId) : ICommand<Result<Unit>>;

public class CancelBookingCommandHandler : ICommandHandler<CancelBookingCommand, Result<Unit>>
{
    private readonly IBookingRepository _bookingRepository;

    public CancelBookingCommandHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<Unit>> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
        if (booking != null)
        {
            booking.Status = "Cancelled";
            await _bookingRepository.UpdateAsync(booking, cancellationToken);
        }
        return Result.Success(Unit.Value);
    }
}
