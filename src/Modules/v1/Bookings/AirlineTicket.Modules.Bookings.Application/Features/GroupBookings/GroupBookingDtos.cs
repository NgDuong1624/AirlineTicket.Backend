using System;
using System.Collections.Generic;

namespace AirlineTicket.Modules.Bookings.Application.Features.GroupBookings;

public sealed record GroupMemberDto(
    Guid Id,
    Guid? UserId,
    string PassengerName,
    string PassengerEmail,
    string? PassengerPhone,
    string? SeatNumber,
    string? ReturnSeatNumber,
    decimal AssignedAmount,
    decimal PaidAmount,
    string PaymentStatus,
    DateTime JoinedAt
);

public sealed record GroupBookingDetailDto(
    Guid Id,
    Guid LeaderUserId,
    Guid FlightId,
    Guid? ReturnFlightId,
    string GroupName,
    string InviteCode,
    decimal TotalAmount,
    decimal PaidAmount,
    string Currency,
    string Status,
    string SplitStrategy,
    DateTime ExpiresAt,
    DateTime CreatedAt,
    List<GroupMemberDto> Members,
    int PercentPaid,
    double TimeRemainingSeconds
);

public sealed record GroupBookingSummaryDto(
    Guid Id,
    string GroupName,
    string InviteCode,
    Guid FlightId,
    string Status,
    decimal TotalAmount,
    decimal PaidAmount,
    string Currency,
    int MemberCount,
    DateTime ExpiresAt,
    DateTime CreatedAt
);

public sealed record CreateGroupBookingResult(
    Guid Id,
    string InviteCode,
    Guid LeaderMemberId
);

public sealed record JoinGroupBookingResult(
    Guid MemberId,
    string InviteCode,
    string PassengerName
);

public sealed record ProcessMemberPaymentResult(
    Guid MemberId,
    string? PaymentUrl,
    string? ClientSecret,
    string Status,
    bool IsGroupFullyPaid
);
