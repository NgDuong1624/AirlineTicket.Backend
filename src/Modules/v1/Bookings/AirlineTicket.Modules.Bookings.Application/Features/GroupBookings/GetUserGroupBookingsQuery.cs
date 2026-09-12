using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Bookings.Application.Contracts;

namespace AirlineTicket.Modules.Bookings.Application.Features.GroupBookings;

public sealed record GetUserGroupBookingsQuery(Guid UserId) : IQuery<Result<List<GroupBookingSummaryDto>>>;

public class GetUserGroupBookingsQueryHandler : IQueryHandler<GetUserGroupBookingsQuery, Result<List<GroupBookingSummaryDto>>>
{
    private readonly IGroupBookingRepository _repository;

    public GetUserGroupBookingsQueryHandler(IGroupBookingRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<GroupBookingSummaryDto>>> Handle(GetUserGroupBookingsQuery request, CancellationToken cancellationToken)
    {
        var list = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);

        var summaries = list.Select(g => new GroupBookingSummaryDto(
            Id: g.Id,
            GroupName: g.GroupName,
            InviteCode: g.InviteCode,
            FlightId: g.FlightId,
            Status: g.Status.ToString(),
            TotalAmount: g.TotalAmount,
            PaidAmount: g.PaidAmount,
            Currency: g.Currency,
            MemberCount: g.Members.Count,
            ExpiresAt: g.ExpiresAt,
            CreatedAt: g.CreatedAt
        )).ToList();

        return Result.Success(summaries);
    }
}
