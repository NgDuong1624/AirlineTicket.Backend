using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using MediatR;

namespace AirlineTicket.Modules.Bookings.Application.Features.Bookings;

public record GetBookingByIdQuery(Guid Id) : IQuery<Result<BookingDto>>;

public class GetBookingByIdQueryHandler : IQueryHandler<GetBookingByIdQuery, Result<BookingDto>>
{
    private readonly IBookingRepository _bookingRepository;

    public GetBookingByIdQueryHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<BookingDto>> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(request.Id, cancellationToken);
        return booking != null
            ? Result.Success(booking)
            : Result.Failure<BookingDto>(new Error("Booking.NotFound", "Booking not found."));
    }
}