using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;

namespace AirlineTicket.Modules.Flights.Application.Features.Airplanes;

public record DeletePartnerAircraftCommand(Guid Id) : ICommand<Result<bool>>;

internal sealed class DeletePartnerAircraftCommandHandler : ICommandHandler<DeletePartnerAircraftCommand, Result<bool>>
{
    public Task<Result<bool>> Handle(DeletePartnerAircraftCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(true));
    }
}
