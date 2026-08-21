using System;

namespace AirlineTicket.Modules.Bookings.Application.Contracts;

public sealed record UserBookingStatsDto(
    int TotalBookings,
    decimal TotalSpent,
    decimal LastMonthSpent,
    decimal LastYearSpent);
