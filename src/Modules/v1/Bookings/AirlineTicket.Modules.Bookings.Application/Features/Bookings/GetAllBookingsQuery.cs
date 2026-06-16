using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using MediatR;

namespace AirlineTicket.Modules.Bookings.Application.Features.Bookings;

public record GetAllBookingsQuery : IRequest<List<BookingDto>>;

public class GetAllBookingsQueryHandler : IRequestHandler<GetAllBookingsQuery, List<BookingDto>>
{
    private readonly IBookingRepository _bookingRepository;

    public GetAllBookingsQueryHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<List<BookingDto>> Handle(GetAllBookingsQuery request, CancellationToken cancellationToken)
    {
        return await _bookingRepository.GetAllAsync(cancellationToken);
    }
}
