using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using AirlineTicket.Modules.Promotions.Domain.Entities;

namespace AirlineTicket.Modules.Promotions.Application.Features.Partner;

public record CreateCampaignPartnerCommand(
    string TitleEn,
    string TitleVi,
    string? BannerUrl,
    string? ContentEn,
    string? ContentVi,
    DateTime StartDate,
    DateTime EndDate,
    bool? IsFeatured,
    Guid AirlineId) : ICommand<Result<Guid>>;

internal sealed class CreateCampaignPartnerCommandHandler : ICommandHandler<CreateCampaignPartnerCommand, Result<Guid>>
{
    private readonly IPromotionRepository promotionRepository;
    public CreateCampaignPartnerCommandHandler(IPromotionRepository promotionRepository) => this.promotionRepository = promotionRepository;

    public async Task<Result<Guid>> Handle(CreateCampaignPartnerCommand request, CancellationToken cancellationToken)
    {
        var campaign = new Campaign
        {
            TitleEn = request.TitleEn,
            TitleVi = request.TitleVi,
            BannerUrl = request.BannerUrl,
            ContentEn = request.ContentEn,
            ContentVi = request.ContentVi,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsFeatured = request.IsFeatured ?? false,
            AirlineId = request.AirlineId
        };
        var id = await promotionRepository.CreateCampaignAsync(campaign, cancellationToken);
        return Result.Success(id);
    }
}
