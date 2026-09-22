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

public record GetActiveCouponsQuery() : IQuery<Result<List<Coupon>>>, ICacheableRequest
{
    public string CacheKey => CacheKeyBuilder.ForQuery<GetActiveCouponsQuery>("all");
    public int CacheDurationMinutes => 15;
}

internal sealed class GetActiveCouponsQueryHandler : IQueryHandler<GetActiveCouponsQuery, Result<List<Coupon>>>
{
    private readonly IPromotionRepository _promotionRepository;
    public GetActiveCouponsQueryHandler(IPromotionRepository promotionRepository) => _promotionRepository = promotionRepository;
    public async Task<Result<List<Coupon>>> Handle(GetActiveCouponsQuery request, CancellationToken cancellationToken)
    {
        var result = await _promotionRepository.GetActiveCouponsAsync(cancellationToken);
        return Result.Success(result);
    }
}
