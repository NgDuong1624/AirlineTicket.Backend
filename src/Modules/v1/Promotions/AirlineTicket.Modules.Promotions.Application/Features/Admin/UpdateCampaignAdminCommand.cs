using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Domain.Constants;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using AirlineTicket.Modules.Promotions.Domain.Entities;
using MediatR;

namespace AirlineTicket.Modules.Promotions.Application.Features.Admin;

public record UpdateCampaignAdminCommand(Guid Id, string Title, string? BannerUrl, string? Content, DateTime StartDate, DateTime EndDate, bool? IsFeatured) : ICommand<Result<Unit>>;

internal sealed class UpdateCampaignAdminCommandHandler : ICommandHandler<UpdateCampaignAdminCommand, Result<Unit>>
{
    private readonly IPromotionRepository _promotionRepository;
    public UpdateCampaignAdminCommandHandler(IPromotionRepository promotionRepository) => _promotionRepository = promotionRepository;

    public async Task<Result<Unit>> Handle(UpdateCampaignAdminCommand request, CancellationToken cancellationToken)
    {
        var campaign = new Campaign
        {
            Id = request.Id,
            Title = request.Title,
            BannerUrl = request.BannerUrl,
            Content = request.Content,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsFeatured = request.IsFeatured ?? false
        };
        var updated = await _promotionRepository.UpdateCampaignAsync(campaign, cancellationToken);
        if (!updated)
            return Result.Failure<Unit>(Error.Create(EndpointErrorCodes.BAD_REQUEST, "Campaign not found."));

        return Result.Success(Unit.Value);
    }
}
