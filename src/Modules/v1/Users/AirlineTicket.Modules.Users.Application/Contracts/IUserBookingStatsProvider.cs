using System;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Users.Application.Contracts;

public interface IUserBookingStatsProvider
{
    Task<UserBookingStatsDto> GetStatsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

public sealed record UserBookingStatsDto(
    int TotalBookings,
    decimal TotalSpent,
    decimal LastMonthSpent,
    decimal LastYearSpent);
