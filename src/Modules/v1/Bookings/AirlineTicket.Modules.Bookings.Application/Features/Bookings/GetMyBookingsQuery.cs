using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using MediatR;

namespace AirlineTicket.Modules.Bookings.Application.Features.Bookings;

public record GetMyBookingsQuery(Guid UserId) : IRequest<List<BookingDto>>;

public class GetMyBookingsQueryHandler : IRequestHandler<GetMyBookingsQuery, List<BookingDto>>
{
    private readonly IBookingRepository _bookingRepository;

    public GetMyBookingsQueryHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<List<BookingDto>> Handle(GetMyBookingsQuery request, CancellationToken cancellationToken)
    {
        return await _bookingRepository.GetByUserIdAsync(request.UserId, cancellationToken);
    }
}