using System;
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

public record GetActivePromotionsQuery() : IQuery<Result<List<Campaign>>>, ICacheableRequest
{
    public string CacheKey => CacheKeyBuilder.ForQuery<GetActivePromotionsQuery>("all");
    public int CacheDurationMinutes => 15;
}

internal sealed class GetActivePromotionsQueryHandler : IQueryHandler<GetActivePromotionsQuery, Result<List<Campaign>>>
{
    private readonly IPromotionRepository _promotionRepository;
    public GetActivePromotionsQueryHandler(IPromotionRepository promotionRepository) => _promotionRepository = promotionRepository;
    public async Task<Result<List<Campaign>>> Handle(GetActivePromotionsQuery request, CancellationToken cancellationToken)
    {
        var result = await _promotionRepository.GetActiveCampaignsAsync(cancellationToken);
        return Result.Success(result);
    }
}
