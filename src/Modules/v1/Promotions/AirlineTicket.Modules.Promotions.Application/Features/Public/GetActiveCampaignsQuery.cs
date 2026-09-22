using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.BuildingBlocks.Behaviors;
using AirlineTicket.BuildingBlocks.Caching;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using AirlineTicket.Modules.Promotions.Domain.Entities;

namespace AirlineTicket.Modules.Promotions.Application.Features.Public;

public record GetActiveCampaignsQuery() : IQuery<Result<List<Campaign>>>, ICacheableRequest
{
    public string CacheKey => CacheKeyBuilder.ForQuery<GetActiveCampaignsQuery>("all");
    public int CacheDurationMinutes => 15;
}

internal sealed class GetActiveCampaignsQueryHandler : IQueryHandler<GetActiveCampaignsQuery, Result<List<Campaign>>>
{
    private readonly IPromotionRepository _promotionRepository;
    public GetActiveCampaignsQueryHandler(IPromotionRepository promotionRepository) => _promotionRepository = promotionRepository;
    public async Task<Result<List<Campaign>>> Handle(GetActiveCampaignsQuery request, CancellationToken cancellationToken)
    {
        var result = await _promotionRepository.GetActiveCampaignsAsync(cancellationToken);
        return Result.Success(result);
    }
}
