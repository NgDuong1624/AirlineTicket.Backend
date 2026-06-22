using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;

namespace AirlineTicket.Modules.Logs.Application.Features;

public record GetAdminLogsQuery(int PageNumber = 1, int PageSize = 10) : IQuery<PagedResult<object>>;

public class GetAdminLogsQueryHandler : IQueryHandler<GetAdminLogsQuery, PagedResult<object>>
{
    public Task<PagedResult<object>> Handle(GetAdminLogsQuery request, CancellationToken cancellationToken)
    {
        var items = new List<object>();
        var result = PagedResult<object>.Success(items, request.PageNumber, request.PageSize, 0);
        return Task.FromResult(result);
    }
}
