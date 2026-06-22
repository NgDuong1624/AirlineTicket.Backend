using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.BuildingBlocks.Behaviors;
using AirlineTicket.BuildingBlocks.Caching;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;

namespace AirlineTicket.Modules.Flights.Application.Features.Airlines;

public record GetAirlinesQuery() : IQuery<Result<List<Airline>>>, ICacheableRequest
{
    public string CacheKey => CacheKeyBuilder.ForQuery<GetAirlinesQuery>("all");
    public int CacheDurationMinutes => 60; // Airlines rarely change
}

internal sealed class GetAirlinesQueryHandler : IQueryHandler<GetAirlinesQuery, Result<List<Airline>>>
{
    private readonly IAirlineRepository _airlineRepository;

    public GetAirlinesQueryHandler(IAirlineRepository airlineRepository)
    {
        _airlineRepository = airlineRepository;
    }

    public async Task<Result<List<Airline>>> Handle(GetAirlinesQuery request, CancellationToken cancellationToken)
    {
        var airlines = await _airlineRepository.GetAllAsync(cancellationToken);
        return Result.Success(airlines);
    }
}
