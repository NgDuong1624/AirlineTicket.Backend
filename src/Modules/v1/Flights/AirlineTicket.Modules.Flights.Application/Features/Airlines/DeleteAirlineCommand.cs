using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.BuildingBlocks.Caching;

namespace AirlineTicket.Modules.Flights.Application.Features.Airlines;

public record DeleteAirlineCommand(Guid Id) : ICommand<Result<bool>>;

internal sealed class DeleteAirlineCommandHandler : ICommandHandler<DeleteAirlineCommand, Result<bool>>
{
    private readonly IAirlineRepository _airlineRepository;
    private readonly ICacheService _cacheService;

    public DeleteAirlineCommandHandler(IAirlineRepository airlineRepository, ICacheService cacheService)
    {
        _airlineRepository = airlineRepository;
        _cacheService = cacheService;
    }

    public async Task<Result<bool>> Handle(DeleteAirlineCommand request, CancellationToken cancellationToken)
    {
        var deleted = await _airlineRepository.DeleteAsync(request.Id, cancellationToken);
        if (!deleted)
        {
            return Result.Failure<bool>(new Error("Airline.NotFound", "Airline not found."));
        }

        await _cacheService.RemoveAsync(CacheKeyBuilder.ForQuery<GetAirlinesQuery>("all"), cancellationToken);

        return Result.Success(true);
    }
}
