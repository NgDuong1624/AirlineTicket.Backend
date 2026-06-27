using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;

namespace AirlineTicket.Modules.Flights.Application.Features.AircraftModels;

public record GetAircraftModelByIdQuery(Guid Id) : IQuery<Result<AircraftModel>>;

internal sealed class GetAircraftModelByIdQueryHandler : IQueryHandler<GetAircraftModelByIdQuery, Result<AircraftModel>>
{
    private readonly IAircraftModelRepository _repository;

    public GetAircraftModelByIdQueryHandler(IAircraftModelRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<AircraftModel>> Handle(GetAircraftModelByIdQuery request, CancellationToken cancellationToken)
    {
        var model = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (model == null)
        {
            return Result.Failure<AircraftModel>(new Error("AircraftModel.NotFound", $"Aircraft model with ID {request.Id} was not found."));
        }
        return Result.Success(model);
    }
}