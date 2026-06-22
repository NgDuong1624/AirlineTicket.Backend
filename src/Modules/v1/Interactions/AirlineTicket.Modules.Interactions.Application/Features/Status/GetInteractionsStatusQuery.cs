using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;

namespace AirlineTicket.Modules.Interactions.Application.Features.Status;

public record GetInteractionsStatusQuery : IQuery<Result<string>>;

public class GetInteractionsStatusQueryHandler : IQueryHandler<GetInteractionsStatusQuery, Result<string>>
{
    public Task<Result<string>> Handle(GetInteractionsStatusQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success("Interactions Module OK"));
    }
}
