using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using MediatR;

namespace AirlineTicket.Modules.Promotions.Application.Features.Partner;

public record DeleteCouponPartnerCommand(Guid Id, Guid AirlineId) : ICommand<Result<Unit>>;

internal sealed class DeleteCouponPartnerCommandHandler : ICommandHandler<DeleteCouponPartnerCommand, Result<Unit>>
{
    private readonly IPromotionRepository _promotionRepository;
    public DeleteCouponPartnerCommandHandler(IPromotionRepository promotionRepository) => _promotionRepository = promotionRepository;

    public async Task<Result<Unit>> Handle(DeleteCouponPartnerCommand request, CancellationToken cancellationToken)
    {
        await _promotionRepository.DeleteCouponAsync(request.Id, request.AirlineId, cancellationToken);
        return Result.Success(Unit.Value);
    }
}
