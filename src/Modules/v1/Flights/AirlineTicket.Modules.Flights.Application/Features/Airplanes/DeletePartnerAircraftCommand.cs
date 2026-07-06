using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.Modules.Flights.Application.Features.Airplanes;

public record DeletePartnerAircraftCommand(Guid Id, Guid AirlineId) : ICommand<Result<bool>>;

internal sealed class DeletePartnerAircraftCommandHandler : ICommandHandler<DeletePartnerAircraftCommand, Result<bool>>
{
    private readonly IAirplaneRepository _airplaneRepository;

    public DeletePartnerAircraftCommandHandler(IAirplaneRepository airplaneRepository)
    {
        _airplaneRepository = airplaneRepository;
    }

    public async Task<Result<bool>> Handle(DeletePartnerAircraftCommand request, CancellationToken cancellationToken)
    {
        await _airplaneRepository.DeleteAsync(request.Id, request.AirlineId, cancellationToken);
        return Result.Success(true);
    }
}
