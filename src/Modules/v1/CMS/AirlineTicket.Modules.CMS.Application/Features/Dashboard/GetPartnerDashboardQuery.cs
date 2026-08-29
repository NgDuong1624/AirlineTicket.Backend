using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.CMS.Application.Contracts;

namespace AirlineTicket.Modules.CMS.Application.Features.Dashboard;

public record GetPartnerDashboardQuery(Guid AirlineId) : IQuery<Result<PartnerDashboardResponse>>;

public record PartnerDashboardResponse(
    List<PartnerStatDto> Stats,
    List<PartnerRecentFlightDto> RecentFlights,
    List<PartnerRecentBookingDto> RecentBookings);

public record PartnerStatDto(string Value);
public record PartnerRecentFlightDto(string Id, string Route, DateTime Time, string Status, string Color);
public record PartnerRecentBookingDto(string Id, string Passenger, string Flight, string Seat, string Class, decimal Amount, DateTime Date);

public class GetPartnerDashboardQueryHandler : IQueryHandler<GetPartnerDashboardQuery, Result<PartnerDashboardResponse>>
{
    private readonly IDashboardRepository _dashboardRepository;

    public GetPartnerDashboardQueryHandler(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<Result<PartnerDashboardResponse>> Handle(GetPartnerDashboardQuery request, CancellationToken cancellationToken)
    {
        var stats = await _dashboardRepository.GetPartnerStatsAsync(request.AirlineId, cancellationToken);
        var recentFlights = await _dashboardRepository.GetPartnerRecentFlightsAsync(request.AirlineId, count: 5, cancellationToken);
        var recentBookings = await _dashboardRepository.GetPartnerRecentBookingsAsync(request.AirlineId, count: 5, cancellationToken);

        var response = new PartnerDashboardResponse(
            stats ?? new List<PartnerStatDto>(),
            recentFlights ?? new List<PartnerRecentFlightDto>(),
            recentBookings ?? new List<PartnerRecentBookingDto>()
        );
        return Result.Success(response);
    }
}
