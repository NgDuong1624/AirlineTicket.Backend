using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.BuildingBlocks.Behaviors;
using AirlineTicket.BuildingBlocks.Caching;
using AirlineTicket.Modules.Promotions.Application.Contracts;

namespace AirlineTicket.Modules.Promotions.Application.Features.Public;

public record GetPromotionByCodeQuery(string Code) : IQuery<Result<PromotionDto>>, ICacheableRequest
{
    public string CacheKey => CacheKeyBuilder.ForQuery<GetPromotionByCodeQuery>(Code);
    public int CacheDurationMinutes => 10;
}

internal sealed class GetPromotionByCodeQueryHandler : IQueryHandler<GetPromotionByCodeQuery, Result<PromotionDto>>
{
    private readonly IPromotionRepository _promotionRepository;
    public GetPromotionByCodeQueryHandler(IPromotionRepository promotionRepository) => _promotionRepository = promotionRepository;
    public async Task<Result<PromotionDto>> Handle(GetPromotionByCodeQuery request, CancellationToken cancellationToken)
    {
        var promotion = await _promotionRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (promotion == null)
        {
            return Result.Failure<PromotionDto>(new Error("Promotion.NotFound", $"Promotion with code {request.Code} was not found."));
        }
        return Result.Success(promotion);
    }
}
