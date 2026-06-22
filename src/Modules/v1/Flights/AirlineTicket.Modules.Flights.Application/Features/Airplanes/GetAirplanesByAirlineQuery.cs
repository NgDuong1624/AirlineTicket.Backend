using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;

namespace AirlineTicket.Modules.Flights.Application.Features.Airplanes;

public record GetAirplanesByAirlineQuery(Guid AirlineId) : IQuery<Result<List<Airplane>>>;

internal sealed class GetAirplanesByAirlineQueryHandler : IQueryHandler<GetAirplanesByAirlineQuery, Result<List<Airplane>>>
{
    private readonly IAirplaneRepository _airplaneRepository;

    public GetAirplanesByAirlineQueryHandler(IAirplaneRepository airplaneRepository)
    {
        _airplaneRepository = airplaneRepository;
    }

    public async Task<Result<List<Airplane>>> Handle(GetAirplanesByAirlineQuery request, CancellationToken cancellationToken)
    {
        var airplanes = await _airplaneRepository.GetByAirlineAsync(request.AirlineId, cancellationToken);
        return Result.Success(airplanes);
    }
}
