using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record DeletePartnerFlightCommand(Guid Id, Guid AirlineId) : ICommand<Result<bool>>;

internal sealed class DeletePartnerFlightCommandHandler : ICommandHandler<DeletePartnerFlightCommand, Result<bool>>
{
    private readonly IFlightRepository _flightRepository;

    public DeletePartnerFlightCommandHandler(IFlightRepository flightRepository)
    {
        _flightRepository = flightRepository;
    }

    public async Task<Result<bool>> Handle(DeletePartnerFlightCommand request, CancellationToken cancellationToken)
    {
        await _flightRepository.DeleteAsync(request.Id, request.AirlineId, cancellationToken);
        return Result.Success(true);
    }
}
