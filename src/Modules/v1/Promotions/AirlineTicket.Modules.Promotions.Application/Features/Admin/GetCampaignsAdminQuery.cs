using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using AirlineTicket.Modules.Promotions.Domain.Entities;

namespace AirlineTicket.Modules.Promotions.Application.Features.Admin;

public record GetCampaignsAdminQuery() : IQuery<Result<List<Campaign>>>;

internal sealed class GetCampaignsAdminQueryHandler : IQueryHandler<GetCampaignsAdminQuery, Result<List<Campaign>>>
{
    private readonly IPromotionRepository _promotionRepository;
    public GetCampaignsAdminQueryHandler(IPromotionRepository promotionRepository) => _promotionRepository = promotionRepository;
    
    public async Task<Result<List<Campaign>>> Handle(GetCampaignsAdminQuery request, CancellationToken cancellationToken)
    {
        var result = await _promotionRepository.GetAllCampaignsAsync(cancellationToken);
        return Result.Success(result);
    }
}
