using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.Modules.Promotions.Application.Contracts;

namespace AirlineTicket.Modules.Promotions.Application.Features.Public;

public record GetActivePromotionsQuery() : IQuery<object>;

internal sealed class GetActivePromotionsQueryHandler : IQueryHandler<GetActivePromotionsQuery, object>
{
    private readonly IPromotionRepository _promotionRepository;
    public GetActivePromotionsQueryHandler(IPromotionRepository promotionRepository) => _promotionRepository = promotionRepository;
    public async Task<object> Handle(GetActivePromotionsQuery request, CancellationToken cancellationToken) => await _promotionRepository.GetActiveCampaignsAsync(cancellationToken);
}
