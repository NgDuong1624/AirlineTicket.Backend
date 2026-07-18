using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using MediatR;

namespace AirlineTicket.Modules.Bookings.Application.Features.Bookings;

public record GetMyBookingsQuery(Guid UserId, int PageIndex = 1, int PageSize = 10) : IQuery<Result<PagedResult<BookingDto>>>;

public class GetMyBookingsQueryHandler : IQueryHandler<GetMyBookingsQuery, Result<PagedResult<BookingDto>>>
{
    private readonly IBookingRepository _bookingRepository;

    public GetMyBookingsQueryHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<PagedResult<BookingDto>>> Handle(GetMyBookingsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _bookingRepository.GetByUserIdAsync(request.UserId, request.PageIndex, request.PageSize, cancellationToken);
        return Result.Success(PagedResult<BookingDto>.Success(items, request.PageIndex, request.PageSize, totalCount));
    }
}