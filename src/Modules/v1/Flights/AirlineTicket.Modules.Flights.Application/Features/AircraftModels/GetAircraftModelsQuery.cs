using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;

namespace AirlineTicket.Modules.Flights.Application.Features.AircraftModels;

public record GetAircraftModelsQuery() : IQuery<Result<List<AircraftModel>>>;

internal sealed class GetAircraftModelsQueryHandler : IQueryHandler<GetAircraftModelsQuery, Result<List<AircraftModel>>>
{
    private readonly IAircraftModelRepository _repository;

    public GetAircraftModelsQueryHandler(IAircraftModelRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<AircraftModel>>> Handle(GetAircraftModelsQuery request, CancellationToken cancellationToken)
    {
        var models = await _repository.GetAllAsync(cancellationToken);
        return Result.Success(models);
    }
}