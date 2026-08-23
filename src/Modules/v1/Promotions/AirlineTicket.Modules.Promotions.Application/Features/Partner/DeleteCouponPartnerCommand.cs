using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Domain.Constants;
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
        var deleted = await _promotionRepository.DeleteCouponAsync(request.Id, request.AirlineId, cancellationToken);
        if (!deleted)
            return Result.Failure<Unit>(Error.Create(EndpointErrorCodes.BAD_REQUEST, "Coupon not found."));

        return Result.Success(Unit.Value);
    }
}
