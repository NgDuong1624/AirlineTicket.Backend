using System;
using AirlineTicket.Modules.Users.Application.Contracts;

namespace AirlineTicket.Modules.Users.Application.Features.Admin;

public record UserDto(
    Guid Id,
    string Email,
    string FullName,
    string? Phone,
    string Role,
    int RoleId,
    Guid? AirlineId,
    string? AirlineName,
    string? AirlineLogoUrl,
    string? LanguagePreference,
    bool IsActive,
    string CreatedAt,
    UserBookingStatsDto? BookingStats = null);
