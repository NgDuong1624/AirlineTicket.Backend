using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using MediatR;

namespace AirlineTicket.Modules.Bookings.Application.Features.Bookings;

public record GetMyBookingsQuery(Guid UserId) : IRequest<object>;

public class GetMyBookingsQueryHandler : IRequestHandler<GetMyBookingsQuery, object>
{
    private readonly IBookingRepository _bookingRepository;

    public GetMyBookingsQueryHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<object> Handle(GetMyBookingsQuery request, CancellationToken cancellationToken)
    {
        return await _bookingRepository.GetByUserIdAsync(request.UserId, cancellationToken);
    }
}
