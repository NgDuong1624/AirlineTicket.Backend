using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using AirlineTicket.Modules.Promotions.Domain.Entities;

namespace AirlineTicket.Modules.Promotions.Application.Features.Partner;

public record GetCouponsPartnerQuery(Guid AirlineId) : IQuery<Result<List<Coupon>>>;

internal sealed class GetCouponsPartnerQueryHandler : IQueryHandler<GetCouponsPartnerQuery, Result<List<Coupon>>>
{
    private readonly IPromotionRepository _promotionRepository;
    public GetCouponsPartnerQueryHandler(IPromotionRepository promotionRepository) => _promotionRepository = promotionRepository;

    public async Task<Result<List<Coupon>>> Handle(GetCouponsPartnerQuery request, CancellationToken cancellationToken)
    {
        var result = await _promotionRepository.GetCouponsByAirlineAsync(request.AirlineId, cancellationToken);
        return Result.Success(result);
    }
}
