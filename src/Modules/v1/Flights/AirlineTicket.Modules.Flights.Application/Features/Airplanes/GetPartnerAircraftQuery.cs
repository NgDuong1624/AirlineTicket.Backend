using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.Modules.Flights.Application.Features.Airplanes;

public record GetPartnerAircraftQuery(Guid AirlineId, int PageIndex = 1, int PageSize = 10) : IQuery<PagedResult<object>>;

internal sealed class GetPartnerAircraftQueryHandler : IQueryHandler<GetPartnerAircraftQuery, PagedResult<object>>
{
    private readonly IAirplaneRepository _airplaneRepository;

    public GetPartnerAircraftQueryHandler(IAirplaneRepository airplaneRepository)
    {
        _airplaneRepository = airplaneRepository;
    }

    public async Task<PagedResult<object>> Handle(GetPartnerAircraftQuery request, CancellationToken cancellationToken)
    {
        var (airplanes, totalCount) = await _airplaneRepository.GetByAirlineAsync(request.AirlineId, request.PageIndex, request.PageSize, cancellationToken);
        return PagedResult<object>.Success(airplanes.Cast<object>().ToList().AsReadOnly(), request.PageIndex, request.PageSize, totalCount);
    }
}
