using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.Modules.Flights.Application.Features.Airlines;

public record DeleteAirlineCommand(Guid Id) : ICommand<Result<bool>>;

internal sealed class DeleteAirlineCommandHandler : ICommandHandler<DeleteAirlineCommand, Result<bool>>
{
    private readonly IAirlineRepository _airlineRepository;

    public DeleteAirlineCommandHandler(IAirlineRepository airlineRepository)
    {
        _airlineRepository = airlineRepository;
    }

    public async Task<Result<bool>> Handle(DeleteAirlineCommand request, CancellationToken cancellationToken)
    {
        await _airlineRepository.DeleteAsync(request.Id, cancellationToken);
        return Result.Success(true);
    }
}
