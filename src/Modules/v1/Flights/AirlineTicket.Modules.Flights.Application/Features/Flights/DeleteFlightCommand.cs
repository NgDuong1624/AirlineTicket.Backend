using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record DeleteFlightCommand(Guid Id) : ICommand<Result<bool>>;

internal sealed class DeleteFlightCommandHandler : ICommandHandler<DeleteFlightCommand, Result<bool>>
{
    private readonly IFlightRepository _flightRepository;

    public DeleteFlightCommandHandler(IFlightRepository flightRepository)
    {
        _flightRepository = flightRepository;
    }

    public async Task<Result<bool>> Handle(DeleteFlightCommand request, CancellationToken cancellationToken)
    {
        await _flightRepository.DeleteAsync(request.Id, null, cancellationToken);
        return Result.Success(true);
    }
}
