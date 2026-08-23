using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Domain.Constants;
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
        var deleted = await _promotionRepository.DeleteCampaignAsync(request.Id, cancellationToken);
        if (!deleted)
            return Result.Failure<Unit>(Error.Create(EndpointErrorCodes.BAD_REQUEST, "Campaign not found."));

        return Result.Success(Unit.Value);
    }
}
