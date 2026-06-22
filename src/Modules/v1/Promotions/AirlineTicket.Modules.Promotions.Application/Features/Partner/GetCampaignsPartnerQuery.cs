using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using AirlineTicket.Modules.Promotions.Domain.Entities;

namespace AirlineTicket.Modules.Promotions.Application.Features.Partner;

public record GetCampaignsPartnerQuery(Guid AirlineId) : IQuery<Result<List<Campaign>>>;

internal sealed class GetCampaignsPartnerQueryHandler : IQueryHandler<GetCampaignsPartnerQuery, Result<List<Campaign>>>
{
    private readonly IPromotionRepository _promotionRepository;
    public GetCampaignsPartnerQueryHandler(IPromotionRepository promotionRepository) => _promotionRepository = promotionRepository;

    public async Task<Result<List<Campaign>>> Handle(GetCampaignsPartnerQuery request, CancellationToken cancellationToken)
    {
        var result = await _promotionRepository.GetCampaignsByAirlineAsync(request.AirlineId, cancellationToken);
        return Result.Success(result);
    }
}
