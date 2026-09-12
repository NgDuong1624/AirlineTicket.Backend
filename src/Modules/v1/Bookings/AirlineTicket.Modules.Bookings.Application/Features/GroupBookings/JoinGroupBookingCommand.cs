using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Domain.Entities;
using AirlineTicket.Modules.Bookings.Domain.Enums;

namespace AirlineTicket.Modules.Bookings.Application.Features.GroupBookings;

public sealed record JoinGroupBookingCommand(
    string InviteCode,
    Guid? UserId,
    string PassengerName,
    string PassengerEmail,
    string? PassengerPhone
) : ICommand<Result<JoinGroupBookingResult>>;

public class JoinGroupBookingCommandHandler : ICommandHandler<JoinGroupBookingCommand, Result<JoinGroupBookingResult>>
{
    private readonly IGroupBookingRepository _repository;

    public JoinGroupBookingCommandHandler(IGroupBookingRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<JoinGroupBookingResult>> Handle(JoinGroupBookingCommand request, CancellationToken cancellationToken)
    {
        var groupBooking = await _repository.GetByInviteCodeAsync(request.InviteCode.Trim().ToUpperInvariant(), cancellationToken);
        if (groupBooking == null)
        {
            return Result.Failure<JoinGroupBookingResult>(new Error("GroupBooking.NotFound", "Group booking lobby not found."));
        }

        if (groupBooking.Status != GroupBookingStatus.Active || DateTime.UtcNow > groupBooking.ExpiresAt)
        {
            groupBooking.MarkExpired();
            await _repository.UpdateAsync(groupBooking, cancellationToken);
            return Result.Failure<JoinGroupBookingResult>(new Error("GroupBooking.Expired", "Group booking session is no longer active."));
        }

        var member = new GroupMember
        {
            Id = Guid.NewGuid(),
            GroupBookingId = groupBooking.Id,
            UserId = request.UserId,
            PassengerName = request.PassengerName.Trim(),
            PassengerEmail = request.PassengerEmail.Trim(),
            PassengerPhone = request.PassengerPhone?.Trim(),
            AssignedAmount = 0,
            PaidAmount = 0,
            PaymentStatus = MemberPaymentStatus.Pending
        };

        try
        {
            groupBooking.AddMember(member);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<JoinGroupBookingResult>(new Error("GroupBooking.JoinFailed", ex.Message));
        }

        await _repository.UpdateAsync(groupBooking, cancellationToken);

        return Result.Success(new JoinGroupBookingResult(member.Id, groupBooking.InviteCode, member.PassengerName));
    }
}
