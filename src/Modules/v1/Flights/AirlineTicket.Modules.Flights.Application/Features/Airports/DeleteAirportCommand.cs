using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.BuildingBlocks.Caching;

namespace AirlineTicket.Modules.Flights.Application.Features.Airports;

public record DeleteAirportCommand(Guid Id) : ICommand<Result<bool>>;

internal sealed class DeleteAirportCommandHandler : ICommandHandler<DeleteAirportCommand, Result<bool>>
{
    private readonly IAirportRepository _airportRepository;
    private readonly ICacheService _cacheService;

    public DeleteAirportCommandHandler(IAirportRepository airportRepository, ICacheService cacheService)
    {
        _airportRepository = airportRepository;
        _cacheService = cacheService;
    }

    public async Task<Result<bool>> Handle(DeleteAirportCommand request, CancellationToken cancellationToken)
    {
        var deleted = await _airportRepository.DeleteAsync(request.Id, cancellationToken);
        if (!deleted)
        {
            return Result.Failure<bool>(new Error("Airport.NotFound", "Airport not found."));
        }

        await _cacheService.RemoveAsync(CacheKeyBuilder.ForQuery<GetAirportsQuery>("all"), cancellationToken);

        return Result.Success(true);
    }
}
