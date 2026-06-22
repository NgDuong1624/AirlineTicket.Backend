using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using AirlineTicket.Modules.Promotions.Domain.Entities;

namespace AirlineTicket.Modules.Promotions.Application.Features.Admin;

public record CreateCampaignAdminCommand(string Title, string? BannerUrl, string? Content, DateTime StartDate, DateTime EndDate, bool? IsFeatured) : ICommand<Result<Guid>>;

internal sealed class CreateCampaignAdminCommandHandler : ICommandHandler<CreateCampaignAdminCommand, Result<Guid>>
{
    private readonly IPromotionRepository _promotionRepository;
    public CreateCampaignAdminCommandHandler(IPromotionRepository promotionRepository) => _promotionRepository = promotionRepository;

    public async Task<Result<Guid>> Handle(CreateCampaignAdminCommand request, CancellationToken cancellationToken)
    {
        var campaign = new Campaign
        {
            Title = request.Title,
            BannerUrl = request.BannerUrl,
            Content = request.Content,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsFeatured = request.IsFeatured ?? false
        };
        var id = await _promotionRepository.CreateCampaignAsync(campaign, cancellationToken);
        return Result.Success(id);
    }
}
