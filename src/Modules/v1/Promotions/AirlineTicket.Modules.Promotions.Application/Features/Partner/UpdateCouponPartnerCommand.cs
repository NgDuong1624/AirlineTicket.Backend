using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Domain.Constants;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using AirlineTicket.Modules.Promotions.Domain.Entities;
using AirlineTicket.Modules.Promotions.Domain.Enums;
using MediatR;

namespace AirlineTicket.Modules.Promotions.Application.Features.Partner;

public record UpdateCouponPartnerCommand(
    Guid Id,
    string Code,
    string? Description,
    int? DiscountType,
    decimal DiscountValue,
    decimal? MinOrderValue,
    decimal? MaxDiscountAmount,
    DateTime StartDate,
    DateTime EndDate,
    int? UsageLimit,
    bool? IsActive,
    Guid AirlineId) : ICommand<Result<Unit>>;

internal sealed class UpdateCouponPartnerCommandHandler : ICommandHandler<UpdateCouponPartnerCommand, Result<Unit>>
{
    private readonly IPromotionRepository _promotionRepository;
    public UpdateCouponPartnerCommandHandler(IPromotionRepository promotionRepository) => _promotionRepository = promotionRepository;

    public async Task<Result<Unit>> Handle(UpdateCouponPartnerCommand request, CancellationToken cancellationToken)
    {
        var coupon = new Coupon
        {
            Id = request.Id,
            Code = request.Code,
            Description = request.Description,
            DiscountType = (DiscountType)(request.DiscountType ?? 0),
            DiscountValue = request.DiscountValue,
            MinOrderValue = request.MinOrderValue,
            MaxDiscountAmount = request.MaxDiscountAmount,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            UsageLimit = request.UsageLimit,
            IsActive = request.IsActive ?? true,
            AirlineId = request.AirlineId
        };
        var updated = await _promotionRepository.UpdateCouponAsync(coupon, request.AirlineId, cancellationToken);
        if (!updated)
            return Result.Failure<Unit>(Error.Create(EndpointErrorCodes.BAD_REQUEST, "Coupon not found."));

        return Result.Success(Unit.Value);
    }
}
