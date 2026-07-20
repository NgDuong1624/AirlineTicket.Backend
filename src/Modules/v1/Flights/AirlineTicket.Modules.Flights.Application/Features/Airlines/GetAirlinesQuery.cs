using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.BuildingBlocks.Behaviors;
using AirlineTicket.BuildingBlocks.Caching;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Application.Features.Airlines;

namespace AirlineTicket.Modules.Flights.Application.Features.Airlines;

public record GetAirlinesQuery(int PageIndex = 1, int PageSize = 100) : IQuery<Result<PagedResult<AirlineDto>>>, ICacheableRequest
{
    public string CacheKey => CacheKeyBuilder.ForQuery<GetAirlinesQuery>($"p{PageIndex}_s{PageSize}");
    public int CacheDurationMinutes => 60; // Airlines rarely change
}

internal sealed class GetAirlinesQueryHandler : IQueryHandler<GetAirlinesQuery, Result<PagedResult<AirlineDto>>>
{
    private readonly IAirlineRepository _airlineRepository;

    public GetAirlinesQueryHandler(IAirlineRepository airlineRepository)
    {
        _airlineRepository = airlineRepository;
    }

    public async Task<Result<PagedResult<AirlineDto>>> Handle(GetAirlinesQuery request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Min(request.PageSize, 100);
        var (items, totalCount) = await _airlineRepository.GetAllAsync(request.PageIndex, pageSize, cancellationToken);
        
        var dtos = items.Select(a => new AirlineDto(a.Id, a.IataCode, a.Name, a.LogoUrl, a.BaseCountry)).ToList();
        
        return Result.Success(PagedResult<AirlineDto>.Success(dtos, request.PageIndex, pageSize, totalCount));
    }
}
