using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;

namespace AirlineTicket.Modules.Flights.Application.Features.Airplanes;

public record GetPartnerAircraftQuery() : IQuery<Result<List<object>>>;

internal sealed class GetPartnerAircraftQueryHandler : IQueryHandler<GetPartnerAircraftQuery, Result<List<object>>>
{
    public Task<Result<List<object>>> Handle(GetPartnerAircraftQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(new List<object>()));
    }
}
