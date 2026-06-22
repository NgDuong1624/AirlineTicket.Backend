using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.Modules.Flights.Application.Features.Airplanes;

public record DeleteAirplaneCommand(Guid Id, Guid AirlineId) : ICommand<Result<bool>>;

internal sealed class DeleteAirplaneCommandHandler : ICommandHandler<DeleteAirplaneCommand, Result<bool>>
{
    private readonly IAirplaneRepository _airplaneRepository;

    public DeleteAirplaneCommandHandler(IAirplaneRepository airplaneRepository)
    {
        _airplaneRepository = airplaneRepository;
    }

    public async Task<Result<bool>> Handle(DeleteAirplaneCommand request, CancellationToken cancellationToken)
    {
        await _airplaneRepository.DeleteAsync(request.Id, request.AirlineId, cancellationToken);
        return Result.Success(true);
    }
}
