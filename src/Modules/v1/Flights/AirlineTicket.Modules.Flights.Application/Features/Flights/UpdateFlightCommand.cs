using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record UpdateFlightCommand(Guid Id) : ICommand<Result<bool>>;

internal sealed class UpdateFlightCommandHandler : ICommandHandler<UpdateFlightCommand, Result<bool>>
{
    public Task<Result<bool>> Handle(UpdateFlightCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(true));
    }
}
