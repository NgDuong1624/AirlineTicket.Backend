using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;

namespace AirlineTicket.Modules.CMS.Application.Features.Settings;

public record GetAdminSettingsQuery : IQuery<Result<AdminSettingsResponse>>;

public record AdminSettingsResponse(decimal ServiceFee, string Currency, bool MaintenanceMode);

public class GetAdminSettingsQueryHandler : IQueryHandler<GetAdminSettingsQuery, Result<AdminSettingsResponse>>
{
    public Task<Result<AdminSettingsResponse>> Handle(GetAdminSettingsQuery request, CancellationToken cancellationToken)
    {
        var response = new AdminSettingsResponse(50000, "VND", false);
        return Task.FromResult(Result.Success(response));
    }
}
