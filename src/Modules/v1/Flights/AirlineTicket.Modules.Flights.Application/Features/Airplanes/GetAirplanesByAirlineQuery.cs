using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;

namespace AirlineTicket.Modules.Flights.Application.Features.Airplanes;

public record GetAirplanesByAirlineQuery(Guid AirlineId, int PageIndex = 1, int PageSize = 10) : IQuery<PagedResult<Airplane>>;

internal sealed class GetAirplanesByAirlineQueryHandler : IQueryHandler<GetAirplanesByAirlineQuery, PagedResult<Airplane>>
{
    private readonly IAirplaneRepository _airplaneRepository;

    public GetAirplanesByAirlineQueryHandler(IAirplaneRepository airplaneRepository)
    {
        _airplaneRepository = airplaneRepository;
    }

    public async Task<PagedResult<Airplane>> Handle(GetAirplanesByAirlineQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _airplaneRepository.GetByAirlineAsync(request.AirlineId, request.PageIndex, request.PageSize, cancellationToken);
        return PagedResult<Airplane>.Success(items.AsReadOnly(), request.PageIndex, request.PageSize, totalCount);
    }
}
