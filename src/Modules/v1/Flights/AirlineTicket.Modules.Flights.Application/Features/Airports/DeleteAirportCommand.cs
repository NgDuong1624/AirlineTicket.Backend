using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.Modules.Flights.Application.Features.Airports;

public record DeleteAirportCommand(Guid Id) : ICommand<Result<bool>>;

internal sealed class DeleteAirportCommandHandler : ICommandHandler<DeleteAirportCommand, Result<bool>>
{
    private readonly IAirportRepository _airportRepository;

    public DeleteAirportCommandHandler(IAirportRepository airportRepository)
    {
        _airportRepository = airportRepository;
    }

    public async Task<Result<bool>> Handle(DeleteAirportCommand request, CancellationToken cancellationToken)
    {
        await _airportRepository.DeleteAsync(request.Id, cancellationToken);
        return Result.Success(true);
    }
}
