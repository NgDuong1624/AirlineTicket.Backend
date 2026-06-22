using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using MediatR;

namespace AirlineTicket.Modules.Promotions.Application.Features.Partner;

public record DeleteCampaignPartnerCommand(Guid Id) : ICommand<Result<Unit>>;

internal sealed class DeleteCampaignPartnerCommandHandler : ICommandHandler<DeleteCampaignPartnerCommand, Result<Unit>>
{
    private readonly IPromotionRepository _promotionRepository;
    public DeleteCampaignPartnerCommandHandler(IPromotionRepository promotionRepository) => _promotionRepository = promotionRepository;

    public async Task<Result<Unit>> Handle(DeleteCampaignPartnerCommand request, CancellationToken cancellationToken)
    {
        await _promotionRepository.DeleteCampaignAsync(request.Id, cancellationToken);
        return Result.Success(Unit.Value);
    }
}
