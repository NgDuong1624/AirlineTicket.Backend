using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record GetAdminFlightsQuery() : IQuery<Result<List<object>>>;

internal sealed class GetAdminFlightsQueryHandler : IQueryHandler<GetAdminFlightsQuery, Result<List<object>>>
{
    public Task<Result<List<object>>> Handle(GetAdminFlightsQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(new List<object>()));
    }
}
