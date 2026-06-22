using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using AirlineTicket.Modules.Promotions.Domain.Entities;

namespace AirlineTicket.Modules.Promotions.Application.Features.Partner;

public record CreateCampaignPartnerCommand(
    string Title,
    string? BannerUrl,
    string? Content,
    DateTime StartDate,
    DateTime EndDate,
    bool? IsFeatured,
    Guid AirlineId) : ICommand<Result<Guid>>;

internal sealed class CreateCampaignPartnerCommandHandler : ICommandHandler<CreateCampaignPartnerCommand, Result<Guid>>
{
    private readonly IPromotionRepository _promotionRepository;
    public CreateCampaignPartnerCommandHandler(IPromotionRepository promotionRepository) => _promotionRepository = promotionRepository;

    public async Task<Result<Guid>> Handle(CreateCampaignPartnerCommand request, CancellationToken cancellationToken)
    {
        var campaign = new Campaign
        {
            Title = request.Title,
            BannerUrl = request.BannerUrl,
            Content = request.Content,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsFeatured = request.IsFeatured ?? false,
            AirlineId = request.AirlineId
        };
        var id = await _promotionRepository.CreateCampaignAsync(campaign, cancellationToken);
        return Result.Success(id);
    }
}
