using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;

namespace AirlineTicket.Modules.Flights.Application.Features.AircraftModels;

public record GetAircraftModelsQuery(int? PageIndex, int? PageSize) : IQuery<Result<PagedResult<AircraftModel>>>;

internal sealed class GetAircraftModelsQueryHandler : IQueryHandler<GetAircraftModelsQuery, Result<PagedResult<AircraftModel>>>
{
    private readonly IAircraftModelRepository _repository;

    public GetAircraftModelsQueryHandler(IAircraftModelRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<AircraftModel>>> Handle(GetAircraftModelsQuery request, CancellationToken cancellationToken)
    {
        var models = await _repository.GetAllAsync(cancellationToken);
        var totalCount = models.Count;
        var items = request.PageIndex.HasValue && request.PageSize.HasValue
            ? models.Skip((request.PageIndex.Value - 1) * request.PageSize.Value).Take(request.PageSize.Value).ToList()
            : models;
        return Result.Success(PagedResult<AircraftModel>.Success(
            items,
            request.PageIndex ?? 1,
            request.PageSize ?? totalCount,
            totalCount));
    }
}