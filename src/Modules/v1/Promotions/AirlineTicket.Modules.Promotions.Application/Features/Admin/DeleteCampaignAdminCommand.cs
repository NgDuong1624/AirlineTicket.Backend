using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using MediatR;

namespace AirlineTicket.Modules.Promotions.Application.Features.Admin;

public record DeleteCampaignAdminCommand(Guid Id) : ICommand<Result<Unit>>;

internal sealed class DeleteCampaignAdminCommandHandler : ICommandHandler<DeleteCampaignAdminCommand, Result<Unit>>
{
    private readonly IPromotionRepository _promotionRepository;
    public DeleteCampaignAdminCommandHandler(IPromotionRepository promotionRepository) => _promotionRepository = promotionRepository;

    public async Task<Result<Unit>> Handle(DeleteCampaignAdminCommand request, CancellationToken cancellationToken)
    {
        await _promotionRepository.DeleteCampaignAsync(request.Id, cancellationToken);
        return Result.Success(Unit.Value);
    }
}
