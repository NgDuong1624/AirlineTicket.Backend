using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using MediatR;

namespace AirlineTicket.Modules.Bookings.Application.Features.Bookings;

public record GetBookingByIdQuery(Guid Id) : IRequest<BookingDto?>;

public class GetBookingByIdQueryHandler : IRequestHandler<GetBookingByIdQuery, BookingDto?>
{
    private readonly IBookingRepository _bookingRepository;

    public GetBookingByIdQueryHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<BookingDto?> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
    {
        return await _bookingRepository.GetByIdAsync(request.Id, cancellationToken);
    }
}