using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.Modules.Flights.Application.Features.Routes;

public record DeleteRouteCommand(Guid Id, Guid AirlineId) : ICommand<Result<bool>>;

internal sealed class DeleteRouteCommandHandler : ICommandHandler<DeleteRouteCommand, Result<bool>>
{
    private readonly IRouteRepository _routeRepository;

    public DeleteRouteCommandHandler(IRouteRepository routeRepository)
    {
        _routeRepository = routeRepository;
    }

    public async Task<Result<bool>> Handle(DeleteRouteCommand request, CancellationToken cancellationToken)
    {
        await _routeRepository.DeleteAsync(request.Id, request.AirlineId, cancellationToken);
        return Result.Success(true);
    }
}
