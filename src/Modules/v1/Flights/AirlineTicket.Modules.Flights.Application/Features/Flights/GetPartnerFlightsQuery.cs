using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record GetPartnerFlightsQuery(Guid AirlineId, int PageIndex = 1, int PageSize = 10) : IQuery<PagedResult<FlightDto>>;

internal sealed class GetPartnerFlightsQueryHandler : IQueryHandler<GetPartnerFlightsQuery, PagedResult<FlightDto>>
{
    private readonly IFlightRepository _flightRepository;

    public GetPartnerFlightsQueryHandler(IFlightRepository flightRepository)
    {
        _flightRepository = flightRepository;
    }

    public async Task<PagedResult<FlightDto>> Handle(GetPartnerFlightsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _flightRepository.GetByAirlineAsync(request.AirlineId, request.PageIndex, request.PageSize, cancellationToken);
        return PagedResult<FlightDto>.Success(items.AsReadOnly(), request.PageIndex, request.PageSize, totalCount);
    }
}
