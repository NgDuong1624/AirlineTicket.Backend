using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record GetPartnerFlightsQuery() : IQuery<Result<List<object>>>;

internal sealed class GetPartnerFlightsQueryHandler : IQueryHandler<GetPartnerFlightsQuery, Result<List<object>>>
{
    public Task<Result<List<object>>> Handle(GetPartnerFlightsQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(new List<object>()));
    }
}
