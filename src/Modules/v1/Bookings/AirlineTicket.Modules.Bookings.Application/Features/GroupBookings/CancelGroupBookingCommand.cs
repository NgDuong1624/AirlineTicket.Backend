using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Domain.Enums;

namespace AirlineTicket.Modules.Bookings.Application.Features.GroupBookings;

public sealed record CancelGroupBookingCommand(
    string InviteCode,
    Guid RequestingUserId
) : ICommand<Result<bool>>;

public class CancelGroupBookingCommandHandler : ICommandHandler<CancelGroupBookingCommand, Result<bool>>
{
    private readonly IGroupBookingRepository _repository;
    private readonly IFlightSeatReservation _seatReservation;

    public CancelGroupBookingCommandHandler(
        IGroupBookingRepository repository,
        IFlightSeatReservation seatReservation)
    {
        _repository = repository;
        _seatReservation = seatReservation;
    }

    public async Task<Result<bool>> Handle(CancelGroupBookingCommand request, CancellationToken cancellationToken)
    {
        var groupBooking = await _repository.GetByInviteCodeAsync(request.InviteCode.Trim().ToUpperInvariant(), cancellationToken);
        if (groupBooking == null)
        {
            return Result.Failure<bool>(new Error("GroupBooking.NotFound", "Group booking lobby not found."));
        }

        if (groupBooking.LeaderUserId != request.RequestingUserId)
        {
            return Result.Failure<bool>(new Error("GroupBooking.Unauthorized", "Only the group trip leader can cancel the booking."));
        }

        try
        {
            groupBooking.Cancel();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<bool>(new Error("GroupBooking.CancelFailed", ex.Message));
        }

        // Release all outbound held seats
        var outboundSeats = groupBooking.Members
            .Where(m => !string.IsNullOrWhiteSpace(m.SeatNumber))
            .Select(m => m.SeatNumber!)
            .ToList();

        if (outboundSeats.Count > 0)
        {
            try
            {
                await _seatReservation.ReleaseSeatsAsync(groupBooking.FlightId, outboundSeats, cancellationToken);
            }
            catch
            {
                // Non-blocking cleanup
            }
        }

        // Release all return held seats if roundtrip
        if (groupBooking.ReturnFlightId.HasValue)
        {
            var returnSeats = groupBooking.Members
                .Where(m => !string.IsNullOrWhiteSpace(m.ReturnSeatNumber))
                .Select(m => m.ReturnSeatNumber!)
                .ToList();

            if (returnSeats.Count > 0)
            {
                try
                {
                    await _seatReservation.ReleaseSeatsAsync(groupBooking.ReturnFlightId.Value, returnSeats, cancellationToken);
                }
                catch
                {
                    // Non-blocking cleanup
                }
            }
        }

        await _repository.UpdateAsync(groupBooking, cancellationToken);

        return Result.Success(true);
    }
}
