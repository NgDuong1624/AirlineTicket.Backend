using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.Modules.Promotions.Application.Contracts;

namespace AirlineTicket.Modules.Promotions.Application.Features.Public;

public record GetPromotionByCodeQuery(string Code) : IQuery<object>;

internal sealed class GetPromotionByCodeQueryHandler : IQueryHandler<GetPromotionByCodeQuery, object>
{
    private readonly IPromotionRepository _promotionRepository;
    public GetPromotionByCodeQueryHandler(IPromotionRepository promotionRepository) => _promotionRepository = promotionRepository;
    public async Task<object> Handle(GetPromotionByCodeQuery request, CancellationToken cancellationToken)
    {
        var promotion = await _promotionRepository.GetByCodeAsync(request.Code, cancellationToken);
        return promotion ?? (object)new { };
    }
}
