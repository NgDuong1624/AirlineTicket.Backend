using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.Modules.Promotions.Application.Contracts;

namespace AirlineTicket.Modules.Promotions.Application.Features.Admin;

public record GetPromotionsAdminQuery() : IQuery<object>;

internal sealed class GetPromotionsAdminQueryHandler : IQueryHandler<GetPromotionsAdminQuery, object>
{
    private readonly IPromotionRepository _promotionRepository;
    public GetPromotionsAdminQueryHandler(IPromotionRepository promotionRepository) => _promotionRepository = promotionRepository;
    public async Task<object> Handle(GetPromotionsAdminQuery request, CancellationToken cancellationToken) => await _promotionRepository.GetAllCouponsAsync(cancellationToken);
}
