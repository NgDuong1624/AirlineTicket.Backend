using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.BuildingBlocks.Caching;

namespace AirlineTicket.Modules.Flights.Application.Features.Routes;

public record DeleteRouteCommand(Guid Id, Guid AirlineId) : ICommand<Result<bool>>;

internal sealed class DeleteRouteCommandHandler : ICommandHandler<DeleteRouteCommand, Result<bool>>
{
    private readonly IRouteRepository _routeRepository;
    private readonly ICacheService _cacheService;

    public DeleteRouteCommandHandler(IRouteRepository routeRepository, ICacheService cacheService)
    {
        _routeRepository = routeRepository;
        _cacheService = cacheService;
    }

    public async Task<Result<bool>> Handle(DeleteRouteCommand request, CancellationToken cancellationToken)
    {
        await _routeRepository.DeleteAsync(request.Id, request.AirlineId, cancellationToken);
        
        await _cacheService.RemoveAsync(CacheKeyBuilder.ForQuery<GetRoutesQuery>("all"), cancellationToken);

        return Result.Success(true);
    }
}
