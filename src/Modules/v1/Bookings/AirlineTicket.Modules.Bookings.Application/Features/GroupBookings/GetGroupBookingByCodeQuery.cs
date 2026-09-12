using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Domain.Enums;

namespace AirlineTicket.Modules.Bookings.Application.Features.GroupBookings;

public sealed record GetGroupBookingByCodeQuery(string InviteCode) : IQuery<Result<GroupBookingDetailDto>>;

public class GetGroupBookingByCodeQueryHandler : IQueryHandler<GetGroupBookingByCodeQuery, Result<GroupBookingDetailDto>>
{
    private readonly IGroupBookingRepository _repository;

    public GetGroupBookingByCodeQueryHandler(IGroupBookingRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<GroupBookingDetailDto>> Handle(GetGroupBookingByCodeQuery request, CancellationToken cancellationToken)
    {
        var groupBooking = await _repository.GetByInviteCodeAsync(request.InviteCode.Trim().ToUpperInvariant(), cancellationToken);
        if (groupBooking == null)
        {
            return Result.Failure<GroupBookingDetailDto>(new Error("GroupBooking.NotFound", "Group booking lobby not found."));
        }

        // Auto-check expiry
        if (groupBooking.Status == GroupBookingStatus.Active && DateTime.UtcNow > groupBooking.ExpiresAt)
        {
            groupBooking.MarkExpired();
            await _repository.UpdateAsync(groupBooking, cancellationToken);
        }

        var memberDtos = groupBooking.Members.Select(m => new GroupMemberDto(
            Id: m.Id,
            UserId: m.UserId,
            PassengerName: m.PassengerName,
            PassengerEmail: m.PassengerEmail,
            PassengerPhone: m.PassengerPhone,
            SeatNumber: m.SeatNumber,
            ReturnSeatNumber: m.ReturnSeatNumber,
            AssignedAmount: m.AssignedAmount,
            PaidAmount: m.PaidAmount,
            PaymentStatus: m.PaymentStatus.ToString(),
            JoinedAt: m.JoinedAt
        )).ToList();

        var percentPaid = groupBooking.TotalAmount > 0
            ? (int)Math.Clamp(Math.Round(groupBooking.PaidAmount / groupBooking.TotalAmount * 100), 0, 100)
            : 0;

        var timeRemaining = Math.Max(0, (groupBooking.ExpiresAt - DateTime.UtcNow).TotalSeconds);

        var dto = new GroupBookingDetailDto(
            Id: groupBooking.Id,
            LeaderUserId: groupBooking.LeaderUserId,
            FlightId: groupBooking.FlightId,
            ReturnFlightId: groupBooking.ReturnFlightId,
            GroupName: groupBooking.GroupName,
            InviteCode: groupBooking.InviteCode,
            TotalAmount: groupBooking.TotalAmount,
            PaidAmount: groupBooking.PaidAmount,
            Currency: groupBooking.Currency,
            Status: groupBooking.Status.ToString(),
            SplitStrategy: groupBooking.SplitStrategy.ToString(),
            ExpiresAt: groupBooking.ExpiresAt,
            CreatedAt: groupBooking.CreatedAt,
            Members: memberDtos,
            PercentPaid: percentPaid,
            TimeRemainingSeconds: timeRemaining
        );

        return Result.Success(dto);
    }
}
