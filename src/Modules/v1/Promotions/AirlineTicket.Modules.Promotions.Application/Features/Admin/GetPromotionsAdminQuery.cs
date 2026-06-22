using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using AirlineTicket.Modules.Promotions.Domain.Entities;

namespace AirlineTicket.Modules.Promotions.Application.Features.Admin;

public record GetPromotionsAdminQuery() : IQuery<Result<List<Coupon>>>;

internal sealed class GetPromotionsAdminQueryHandler : IQueryHandler<GetPromotionsAdminQuery, Result<List<Coupon>>>
{
    private readonly IPromotionRepository _promotionRepository;
    public GetPromotionsAdminQueryHandler(IPromotionRepository promotionRepository) => _promotionRepository = promotionRepository;
    public async Task<Result<List<Coupon>>> Handle(GetPromotionsAdminQuery request, CancellationToken cancellationToken)
    {
        var result = await _promotionRepository.GetAllCouponsAsync(cancellationToken);
        return Result.Success(result);
    }
}
