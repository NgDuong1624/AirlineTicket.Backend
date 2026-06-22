using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;

namespace AirlineTicket.Modules.Notifications.Application.Features.Status;

public record GetNotificationsStatusQuery : IQuery<Result<string>>;

public class GetNotificationsStatusQueryHandler : IQueryHandler<GetNotificationsStatusQuery, Result<string>>
{
    public Task<Result<string>> Handle(GetNotificationsStatusQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success("Notifications Module OK"));
    }
}
