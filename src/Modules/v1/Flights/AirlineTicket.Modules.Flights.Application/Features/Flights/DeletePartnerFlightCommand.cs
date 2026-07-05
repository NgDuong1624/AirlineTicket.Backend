using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record DeletePartnerFlightCommand(Guid Id, Guid AirlineId) : ICommand<Result<bool>>;

internal sealed class DeletePartnerFlightCommandHandler : ICommandHandler<DeletePartnerFlightCommand, Result<bool>>
{
    public Task<Result<bool>> Handle(DeletePartnerFlightCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(true));
    }
}
