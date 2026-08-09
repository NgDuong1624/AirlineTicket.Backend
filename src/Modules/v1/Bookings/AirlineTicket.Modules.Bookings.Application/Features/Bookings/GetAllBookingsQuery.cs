using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using MediatR;

namespace AirlineTicket.Modules.Bookings.Application.Features.Bookings;

public record GetAllBookingsQuery(
    int PageIndex = 1, 
    int PageSize = 10,
    string? Search = null,
    string? Status = null,
    DateTime? Date = null) : IQuery<PagedResult<BookingDto>>;

public class GetAllBookingsQueryHandler : IQueryHandler<GetAllBookingsQuery, PagedResult<BookingDto>>
{
    private readonly IBookingRepository _bookingRepository;

    public GetAllBookingsQueryHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<PagedResult<BookingDto>> Handle(GetAllBookingsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _bookingRepository.GetAllAsync(
            request.PageIndex, 
            request.PageSize, 
            request.Search,
            request.Status,
            request.Date,
            cancellationToken);
        return PagedResult<BookingDto>.Success(items.AsReadOnly(), request.PageIndex, request.PageSize, totalCount);
    }
}
