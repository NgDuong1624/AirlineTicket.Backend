using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using AirlineTicket.Modules.Promotions.Domain.Entities;

namespace AirlineTicket.Modules.Promotions.Application.Features.Admin;

public record GetCampaignsAdminQuery(int PageIndex = 1, int PageSize = 10) : IQuery<Result<PagedResult<Campaign>>>;

internal sealed class GetCampaignsAdminQueryHandler : IQueryHandler<GetCampaignsAdminQuery, Result<PagedResult<Campaign>>>
{
    private readonly IPromotionRepository _promotionRepository;
    public GetCampaignsAdminQueryHandler(IPromotionRepository promotionRepository) => _promotionRepository = promotionRepository;
    
    public async Task<Result<PagedResult<Campaign>>> Handle(GetCampaignsAdminQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _promotionRepository.GetAllCampaignsAsync(request.PageIndex, request.PageSize, cancellationToken);
        return Result.Success(PagedResult<Campaign>.Success(items, request.PageIndex, request.PageSize, totalCount));
    }
}
