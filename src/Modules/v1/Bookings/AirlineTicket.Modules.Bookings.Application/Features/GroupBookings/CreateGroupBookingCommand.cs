using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Domain.Entities;
using AirlineTicket.Modules.Bookings.Domain.Enums;

namespace AirlineTicket.Modules.Bookings.Application.Features.GroupBookings;

public sealed record CreateGroupBookingCommand(
    Guid LeaderUserId,
    string LeaderName,
    string LeaderEmail,
    string? LeaderPhone,
    Guid FlightId,
    Guid? ReturnFlightId,
    string GroupName,
    SplitStrategy SplitStrategy,
    decimal TotalAmount,
    string Currency,
    string? LeaderSeatNumber
) : ICommand<Result<CreateGroupBookingResult>>;

public class CreateGroupBookingCommandHandler : ICommandHandler<CreateGroupBookingCommand, Result<CreateGroupBookingResult>>
{
    private readonly IGroupBookingRepository _repository;
    private readonly IFlightSeatReservation _seatReservation;

    public CreateGroupBookingCommandHandler(
        IGroupBookingRepository repository,
        IFlightSeatReservation seatReservation)
    {
        _repository = repository;
        _seatReservation = seatReservation;
    }

    public async Task<Result<CreateGroupBookingResult>> Handle(CreateGroupBookingCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.GroupName))
        {
            return Result.Failure<CreateGroupBookingResult>(new Error("GroupBooking.InvalidName", "Group name is required."));
        }

        if (string.IsNullOrWhiteSpace(request.LeaderEmail))
        {
            return Result.Failure<CreateGroupBookingResult>(new Error("GroupBooking.InvalidEmail", "Leader email is required."));
        }

        // Generate invite code: 6 chars uppercase alphanumeric
        var inviteCode = GenerateInviteCode();
        while (await _repository.GetByInviteCodeAsync(inviteCode, cancellationToken) != null)
        {
            inviteCode = GenerateInviteCode();
        }

        var groupBooking = new GroupBooking
        {
            Id = Guid.NewGuid(),
            LeaderUserId = request.LeaderUserId,
            FlightId = request.FlightId,
            ReturnFlightId = request.ReturnFlightId,
            GroupName = request.GroupName.Trim(),
            InviteCode = inviteCode,
            TotalAmount = request.TotalAmount,
            PaidAmount = 0,
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "VND" : request.Currency.ToUpperInvariant(),
            Status = GroupBookingStatus.Active,
            SplitStrategy = request.SplitStrategy,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        var leaderMember = new GroupMember
        {
            Id = Guid.NewGuid(),
            GroupBookingId = groupBooking.Id,
            UserId = request.LeaderUserId,
            PassengerName = request.LeaderName,
            PassengerEmail = request.LeaderEmail,
            PassengerPhone = request.LeaderPhone,
            SeatNumber = request.LeaderSeatNumber,
            AssignedAmount = request.TotalAmount,
            PaidAmount = 0,
            PaymentStatus = MemberPaymentStatus.Pending
        };

        if (!string.IsNullOrWhiteSpace(request.LeaderSeatNumber))
        {
            try
            {
                await _seatReservation.ReserveSeatsAsync(
                    request.FlightId,
                    new[] { request.LeaderSeatNumber },
                    cancellationToken);
            }
            catch (SeatUnavailableException ex)
            {
                return Result.Failure<CreateGroupBookingResult>(new Error("Seat.Unavailable", ex.Message));
            }
        }

        groupBooking.AddMember(leaderMember);
        await _repository.AddAsync(groupBooking, cancellationToken);

        return Result.Success(new CreateGroupBookingResult(groupBooking.Id, groupBooking.InviteCode, leaderMember.Id));
    }

    private static string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var bytes = RandomNumberGenerator.GetBytes(6);
        var result = new char[6];
        for (int i = 0; i < 6; i++)
        {
            result[i] = chars[bytes[i] % chars.Length];
        }
        return new string(result);
    }
}
