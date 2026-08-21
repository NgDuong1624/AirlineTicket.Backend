using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Bookings.Application.Contracts;

namespace AirlineTicket.Modules.Bookings.Application.Features.Bookings;

public record GetUserBookingStatsQuery(Guid UserId) : IQuery<Result<UserBookingStatsDto>>;

public class GetUserBookingStatsQueryHandler : IQueryHandler<GetUserBookingStatsQuery, Result<UserBookingStatsDto>>
{
    private readonly IBookingRepository _bookingRepository;

    public GetUserBookingStatsQueryHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<UserBookingStatsDto>> Handle(GetUserBookingStatsQuery request, CancellationToken cancellationToken)
    {
        var stats = await _bookingRepository.GetStatsByUserIdAsync(request.UserId, cancellationToken);
        return Result.Success(stats);
    }
}
