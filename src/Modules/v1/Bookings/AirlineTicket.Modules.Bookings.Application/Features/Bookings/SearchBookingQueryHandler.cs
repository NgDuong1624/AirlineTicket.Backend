using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Bookings.Application.Contracts;

namespace AirlineTicket.Modules.Bookings.Application.Features.Bookings;

public class SearchBookingQueryHandler : IQueryHandler<SearchBookingQuery, Result<BookingDetailDto>>
{
    private readonly IBookingRepository _bookingRepository;

    public SearchBookingQueryHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<BookingDetailDto>> Handle(SearchBookingQuery request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetDetailByPnrAsync(request.PnrCode, cancellationToken);
        
        if (booking == null)
            return Result.Failure<BookingDetailDto>(new Error("Booking.NotFound", "Booking not found"));

        return Result.Success(booking);
    }
}
