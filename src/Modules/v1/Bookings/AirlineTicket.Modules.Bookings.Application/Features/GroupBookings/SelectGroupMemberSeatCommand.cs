using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Domain.Enums;

namespace AirlineTicket.Modules.Bookings.Application.Features.GroupBookings;

public sealed record SelectGroupMemberSeatCommand(
    string InviteCode,
    Guid MemberId,
    string SeatNumber,
    bool IsReturn = false
) : ICommand<Result<bool>>;

public class SelectGroupMemberSeatCommandHandler : ICommandHandler<SelectGroupMemberSeatCommand, Result<bool>>
{
    private readonly IGroupBookingRepository _repository;
    private readonly IFlightSeatReservation _seatReservation;

    public SelectGroupMemberSeatCommandHandler(
        IGroupBookingRepository repository,
        IFlightSeatReservation seatReservation)
    {
        _repository = repository;
        _seatReservation = seatReservation;
    }

    public async Task<Result<bool>> Handle(SelectGroupMemberSeatCommand request, CancellationToken cancellationToken)
    {
        var groupBooking = await _repository.GetByInviteCodeAsync(request.InviteCode.Trim().ToUpperInvariant(), cancellationToken);
        if (groupBooking == null)
        {
            return Result.Failure<bool>(new Error("GroupBooking.NotFound", "Group booking lobby not found."));
        }

        if (groupBooking.Status != GroupBookingStatus.Active || DateTime.UtcNow > groupBooking.ExpiresAt)
        {
            return Result.Failure<bool>(new Error("GroupBooking.Inactive", "Group booking is not active."));
        }

        var member = groupBooking.Members.FirstOrDefault(m => m.Id == request.MemberId);
        if (member == null)
        {
            return Result.Failure<bool>(new Error("GroupMember.NotFound", "Member not found in group."));
        }

        if (member.PaymentStatus == MemberPaymentStatus.Paid)
        {
            return Result.Failure<bool>(new Error("GroupMember.AlreadyPaid", "Cannot change seat after payment."));
        }

        var targetFlightId = request.IsReturn ? groupBooking.ReturnFlightId : groupBooking.FlightId;
        if (!targetFlightId.HasValue)
        {
            return Result.Failure<bool>(new Error("Flight.NotFound", "Target flight does not exist for this group."));
        }

        var oldSeat = request.IsReturn ? member.ReturnSeatNumber : member.SeatNumber;

        // Reserve new seat
        decimal seatPrice = 0;
        try
        {
            var reserved = await _seatReservation.ReserveSeatsAsync(
                targetFlightId.Value,
                new[] { request.SeatNumber },
                cancellationToken);

            if (reserved.TryGetValue(request.SeatNumber, out var seatInfo))
            {
                seatPrice = seatInfo.Price;
            }
        }
        catch (SeatUnavailableException ex)
        {
            return Result.Failure<bool>(new Error("Seat.Unavailable", ex.Message));
        }

        // Release old seat if member had one previously
        if (!string.IsNullOrWhiteSpace(oldSeat) && !oldSeat.Equals(request.SeatNumber, StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                await _seatReservation.ReleaseSeatsAsync(
                    targetFlightId.Value,
                    new[] { oldSeat },
                    cancellationToken);
            }
            catch
            {
                // Non-blocking release log
            }
        }

        try
        {
            groupBooking.SelectMemberSeat(request.MemberId, request.SeatNumber, seatPrice, request.IsReturn);
        }
        catch (InvalidOperationException ex)
        {
            // Rollback reserved seat
            await _seatReservation.ReleaseSeatsAsync(targetFlightId.Value, new[] { request.SeatNumber }, cancellationToken);
            return Result.Failure<bool>(new Error("GroupBooking.SeatFailed", ex.Message));
        }

        await _repository.UpdateAsync(groupBooking, cancellationToken);

        return Result.Success(true);
    }
}
