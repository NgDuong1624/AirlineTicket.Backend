using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;

namespace AirlineTicket.Modules.CMS.Application.Features.Dashboard;

public record GetPartnerDashboardQuery : IQuery<Result<PartnerDashboardResponse>>;

public record PartnerDashboardResponse(
    List<PartnerStatDto> Stats,
    List<object> RecentFlights,
    List<object> RecentBookings);

public record PartnerStatDto(string Value);

public class GetPartnerDashboardQueryHandler : IQueryHandler<GetPartnerDashboardQuery, Result<PartnerDashboardResponse>>
{
    public Task<Result<PartnerDashboardResponse>> Handle(GetPartnerDashboardQuery request, CancellationToken cancellationToken)
    {
        var response = new PartnerDashboardResponse(
            new List<PartnerStatDto>
            {
                new("0"),
                new("0"),
                new("0")
            },
            new List<object>(),
            new List<object>()
        );
        return Task.FromResult(Result.Success(response));
    }
}
