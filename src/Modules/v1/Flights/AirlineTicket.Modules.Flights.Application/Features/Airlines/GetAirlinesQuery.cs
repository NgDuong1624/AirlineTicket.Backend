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

public record GetAirlinesQuery(int PageIndex = 1, int PageSize = 10) : IQuery<Result<PagedResult<Airline>>>, ICacheableRequest
{
    public string CacheKey => CacheKeyBuilder.ForQuery<GetAirlinesQuery>($"p{PageIndex}_s{PageSize}");
    public int CacheDurationMinutes => 60; // Airlines rarely change
}

internal sealed class GetAirlinesQueryHandler : IQueryHandler<GetAirlinesQuery, Result<PagedResult<Airline>>>
{
    private readonly IAirlineRepository _airlineRepository;

    public GetAirlinesQueryHandler(IAirlineRepository airlineRepository)
    {
        _airlineRepository = airlineRepository;
    }

    public async Task<Result<PagedResult<Airline>>> Handle(GetAirlinesQuery request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Min(request.PageSize, 100);
        var (items, totalCount) = await _airlineRepository.GetAllAsync(request.PageIndex, pageSize, cancellationToken);
        return Result.Success(PagedResult<Airline>.Success(items, request.PageIndex, pageSize, totalCount));
    }
}
