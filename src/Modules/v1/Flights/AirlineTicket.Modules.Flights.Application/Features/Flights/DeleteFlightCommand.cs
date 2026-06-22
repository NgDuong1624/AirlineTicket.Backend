using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record DeleteFlightCommand(Guid Id) : ICommand<Result<bool>>;

internal sealed class DeleteFlightCommandHandler : ICommandHandler<DeleteFlightCommand, Result<bool>>
{
    public Task<Result<bool>> Handle(DeleteFlightCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(true));
    }
}
