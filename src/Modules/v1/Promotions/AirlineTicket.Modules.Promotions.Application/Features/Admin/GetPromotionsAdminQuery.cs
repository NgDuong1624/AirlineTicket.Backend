using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using AirlineTicket.Modules.Promotions.Domain.Entities;

namespace AirlineTicket.Modules.Promotions.Application.Features.Admin;

public record GetPromotionsAdminQuery(int PageIndex = 1, int PageSize = 10) : IQuery<Result<PagedResult<Coupon>>>;

internal sealed class GetPromotionsAdminQueryHandler : IQueryHandler<GetPromotionsAdminQuery, Result<PagedResult<Coupon>>>
{
    private readonly IPromotionRepository _promotionRepository;
    public GetPromotionsAdminQueryHandler(IPromotionRepository promotionRepository) => _promotionRepository = promotionRepository;
    public async Task<Result<PagedResult<Coupon>>> Handle(GetPromotionsAdminQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _promotionRepository.GetAllCouponsAsync(request.PageIndex, request.PageSize, cancellationToken);
        return Result.Success(PagedResult<Coupon>.Success(items, request.PageIndex, request.PageSize, totalCount));
    }
}
