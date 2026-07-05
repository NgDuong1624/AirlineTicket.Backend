using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record UpdatePartnerFlightCommand(Guid Id, Guid AirlineId) : ICommand<Result<bool>>;

internal sealed class UpdatePartnerFlightCommandHandler : ICommandHandler<UpdatePartnerFlightCommand, Result<bool>>
{
    public Task<Result<bool>> Handle(UpdatePartnerFlightCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(true));
    }
}
