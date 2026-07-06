using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.Modules.Flights.Application.Features.Airplanes;

public record GetPartnerAircraftQuery(Guid AirlineId) : IQuery<Result<List<object>>>;

internal sealed class GetPartnerAircraftQueryHandler : IQueryHandler<GetPartnerAircraftQuery, Result<List<object>>>
{
    private readonly IAirplaneRepository _airplaneRepository;

    public GetPartnerAircraftQueryHandler(IAirplaneRepository airplaneRepository)
    {
        _airplaneRepository = airplaneRepository;
    }

    public async Task<Result<List<object>>> Handle(GetPartnerAircraftQuery request, CancellationToken cancellationToken)
    {
        var airplanes = await _airplaneRepository.GetByAirlineAsync(request.AirlineId, cancellationToken);
        return Result.Success(airplanes.Cast<object>().ToList());
    }
}
