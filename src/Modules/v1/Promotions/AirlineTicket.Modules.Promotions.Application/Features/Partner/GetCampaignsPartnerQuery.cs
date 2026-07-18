using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using AirlineTicket.Modules.Promotions.Domain.Entities;

namespace AirlineTicket.Modules.Promotions.Application.Features.Partner;

public record GetCampaignsPartnerQuery(Guid AirlineId, int PageIndex = 1, int PageSize = 10) : IQuery<Result<PagedResult<Campaign>>>;

internal sealed class GetCampaignsPartnerQueryHandler : IQueryHandler<GetCampaignsPartnerQuery, Result<PagedResult<Campaign>>>
{
    private readonly IPromotionRepository _promotionRepository;
    public GetCampaignsPartnerQueryHandler(IPromotionRepository promotionRepository) => _promotionRepository = promotionRepository;

    public async Task<Result<PagedResult<Campaign>>> Handle(GetCampaignsPartnerQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _promotionRepository.GetCampaignsByAirlineAsync(request.AirlineId, request.PageIndex, request.PageSize, cancellationToken);
        return Result.Success(PagedResult<Campaign>.Success(items, request.PageIndex, request.PageSize, totalCount));
    }
}
