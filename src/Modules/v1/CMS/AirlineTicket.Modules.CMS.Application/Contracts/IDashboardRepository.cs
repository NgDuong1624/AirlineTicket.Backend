using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.CMS.Application.Features.Dashboard;

namespace AirlineTicket.Modules.CMS.Application.Contracts;

public interface IDashboardRepository
{
    Task<AdminDashboardStatsDto?> GetAdminStatsAsync(CancellationToken cancellationToken = default);
    Task<List<AdminDashboardPartnerDto>> GetRecentPartnersAsync(int count = 5, CancellationToken cancellationToken = default);
    Task<List<AdminDashboardLogDto>> GetCriticalLogsAsync(int count = 4, CancellationToken cancellationToken = default);
    Task<List<PartnerStatDto>> GetPartnerStatsAsync(Guid airlineId, CancellationToken cancellationToken = default);
    Task<List<PartnerRecentFlightDto>> GetPartnerRecentFlightsAsync(Guid airlineId, int count = 5, CancellationToken cancellationToken = default);
    Task<List<PartnerRecentBookingDto>> GetPartnerRecentBookingsAsync(Guid airlineId, int count = 5, CancellationToken cancellationToken = default);
}

public record AdminDashboardStatsDto(decimal TotalRevenue, int TotalBookings, int NewUsers, int TotalFlights);
