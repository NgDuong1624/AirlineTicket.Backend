using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;

namespace AirlineTicket.Modules.Flights.Application.Features.Airplanes;

public record UpdatePartnerAircraftCommand(Guid Id) : ICommand<Result<bool>>;

internal sealed class UpdatePartnerAircraftCommandHandler : ICommandHandler<UpdatePartnerAircraftCommand, Result<bool>>
{
    public Task<Result<bool>> Handle(UpdatePartnerAircraftCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(true));
    }
}
