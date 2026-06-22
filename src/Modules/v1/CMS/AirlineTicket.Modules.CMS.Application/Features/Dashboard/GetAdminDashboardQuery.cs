using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;

namespace AirlineTicket.Modules.CMS.Application.Features.Dashboard;

public record GetAdminDashboardQuery : IQuery<Result<AdminDashboardResponse>>;

public record AdminDashboardResponse(long TotalRevenue, int TotalBookings, int NewUsers);

public class GetAdminDashboardQueryHandler : IQueryHandler<GetAdminDashboardQuery, Result<AdminDashboardResponse>>
{
    public Task<Result<AdminDashboardResponse>> Handle(GetAdminDashboardQuery request, CancellationToken cancellationToken)
    {
        var response = new AdminDashboardResponse(1500000000, 1250, 450);
        return Task.FromResult(Result.Success(response));
    }
}
