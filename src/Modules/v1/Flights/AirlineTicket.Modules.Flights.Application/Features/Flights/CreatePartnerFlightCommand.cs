using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record CreatePartnerFlightCommand() : ICommand<Result<Guid>>;

internal sealed class CreatePartnerFlightCommandHandler : ICommandHandler<CreatePartnerFlightCommand, Result<Guid>>
{
    public Task<Result<Guid>> Handle(CreatePartnerFlightCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(Guid.NewGuid()));
    }
}
