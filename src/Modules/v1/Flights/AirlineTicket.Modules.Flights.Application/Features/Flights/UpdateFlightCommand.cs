using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record UpdateFlightCommand(Guid Id) : ICommand<Result<bool>>;

internal sealed class UpdateFlightCommandHandler : ICommandHandler<UpdateFlightCommand, Result<bool>>
{
    private readonly IFlightRepository _flightRepository;

    public UpdateFlightCommandHandler(IFlightRepository flightRepository)
    {
        _flightRepository = flightRepository;
    }

    public async Task<Result<bool>> Handle(UpdateFlightCommand request, CancellationToken cancellationToken)
    {
        var flight = await _flightRepository.GetByIdAsync(request.Id, cancellationToken);
        if (flight is null)
        {
            return Result.Failure<bool>(new Error("Flight.NotFound", "Flight not found"));
        }
        
        return Result.Success(true);
    }
}
