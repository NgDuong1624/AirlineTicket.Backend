using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.CMS.Application.Contracts;

namespace AirlineTicket.Modules.CMS.Application.Features.Dashboard;

public record GetAdminDashboardQuery : IQuery<Result<AdminDashboardResponse>>;

public record AdminDashboardResponse(
    List<AdminDashboardStatDto> Stats,
    List<AdminDashboardPartnerDto> RecentPartners,
    List<AdminDashboardLogDto> CriticalLogs
);

public record AdminDashboardStatDto(string Value, string Trend);
public record AdminDashboardPartnerDto(string Name, string Code, int FlightsCount, string Status, string Joined);
public record AdminDashboardLogDto(string Time, string Type, string LabelKey);

public class GetAdminDashboardQueryHandler : IQueryHandler<GetAdminDashboardQuery, Result<AdminDashboardResponse>>
{
    private readonly IDashboardRepository _dashboardRepository;

    public GetAdminDashboardQueryHandler(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<Result<AdminDashboardResponse>> Handle(GetAdminDashboardQuery request, CancellationToken cancellationToken)
    {
        var statsData = await _dashboardRepository.GetAdminStatsAsync(cancellationToken);
        var partners = await _dashboardRepository.GetRecentPartnersAsync(5, cancellationToken);
        var logs = await _dashboardRepository.GetCriticalLogsAsync(4, cancellationToken);

        var stats = new List<AdminDashboardStatDto>
        {
            new(statsData != null ? $"${statsData.TotalRevenue:N0}" : "$0", "up"),
            new(statsData != null ? statsData.TotalBookings.ToString("N0") : "0", "up"),
            new(statsData != null ? $"{statsData.NewUsers}" : "0", "neutral"),
            new(statsData != null ? statsData.TotalFlights.ToString("N0") : "0", "down")
        };

        var response = new AdminDashboardResponse(stats, partners, logs);
        return Result.Success(response);
    }
}
