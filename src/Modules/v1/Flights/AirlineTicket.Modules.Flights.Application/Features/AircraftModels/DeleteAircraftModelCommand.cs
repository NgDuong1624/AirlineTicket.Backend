using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.Modules.Flights.Application.Features.AircraftModels;

public record DeleteAircraftModelCommand(Guid Id) : ICommand<Result<bool>>;

internal sealed class DeleteAircraftModelCommandHandler : ICommandHandler<DeleteAircraftModelCommand, Result<bool>>
{
    private readonly IAircraftModelRepository _repository;

    public DeleteAircraftModelCommandHandler(IAircraftModelRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> Handle(DeleteAircraftModelCommand request, CancellationToken cancellationToken)
    {
        var deleted = await _repository.DeleteAsync(request.Id, cancellationToken);
        if (!deleted)
        {
            return Result.Failure<bool>(new Error("AircraftModel.NotFound", "Aircraft model not found."));
        }
        return Result.Success(true);
    }
}