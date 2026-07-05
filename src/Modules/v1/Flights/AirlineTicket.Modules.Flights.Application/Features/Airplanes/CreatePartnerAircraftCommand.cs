using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;

namespace AirlineTicket.Modules.Flights.Application.Features.Airplanes;

public record CreatePartnerAircraftCommand(Guid AirlineId) : ICommand<Result<Guid>>;

internal sealed class CreatePartnerAircraftCommandHandler : ICommandHandler<CreatePartnerAircraftCommand, Result<Guid>>
{
    public Task<Result<Guid>> Handle(CreatePartnerAircraftCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(Guid.NewGuid()));
    }
}
