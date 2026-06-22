using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using AirlineTicket.Modules.Promotions.Domain.Entities;
using AirlineTicket.Modules.Promotions.Domain.Enums;

namespace AirlineTicket.Modules.Promotions.Application.Features.Partner;

public record CreateCouponPartnerCommand(
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
    Guid AirlineId) : ICommand<Result<Guid>>;

internal sealed class CreateCouponPartnerCommandHandler : ICommandHandler<CreateCouponPartnerCommand, Result<Guid>>
{
    private readonly IPromotionRepository _promotionRepository;
    public CreateCouponPartnerCommandHandler(IPromotionRepository promotionRepository) => _promotionRepository = promotionRepository;

    public async Task<Result<Guid>> Handle(CreateCouponPartnerCommand request, CancellationToken cancellationToken)
    {
        var coupon = new Coupon
        {
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
        var id = await _promotionRepository.CreateCouponAsync(coupon, cancellationToken);
        return Result.Success(id);
    }
}
