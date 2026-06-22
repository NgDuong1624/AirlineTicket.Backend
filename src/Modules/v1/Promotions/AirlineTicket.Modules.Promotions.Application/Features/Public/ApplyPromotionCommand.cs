using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using MediatR;

namespace AirlineTicket.Modules.Promotions.Application.Features.Public;

public record ApplyPromotionResponse(decimal DiscountAmount, decimal FinalAmount);

public record ApplyPromotionCommand(string PromoCode, Guid FlightId, decimal OriginalAmount) : ICommand<Result<ApplyPromotionResponse>>;

public class ApplyPromotionCommandHandler : ICommandHandler<ApplyPromotionCommand, Result<ApplyPromotionResponse>>
{
    private readonly IPromotionRepository _promotionRepository;

    public ApplyPromotionCommandHandler(IPromotionRepository promotionRepository)
    {
        _promotionRepository = promotionRepository;
    }

    public async Task<Result<ApplyPromotionResponse>> Handle(ApplyPromotionCommand request, CancellationToken cancellationToken)
    {
        var promo = await _promotionRepository.GetByCodeAsync(request.PromoCode, cancellationToken);
        if (promo == null || promo.EndDate < DateTime.UtcNow || promo.CurrentUsage >= promo.MaxUsage)
        {
            return Result.Failure<ApplyPromotionResponse>(new Error("PROMOTION_INVALID", "Mã giảm giá không hợp lệ hoặc đã hết hạn."));
        }

        decimal discountAmount = 0;
        if (promo.DiscountType == "Percentage")
        {
            discountAmount = request.OriginalAmount * (promo.DiscountValue / 100);
        }
        else if (promo.DiscountType == "FixedAmount")
        {
            discountAmount = promo.DiscountValue;
        }

        var finalAmount = request.OriginalAmount - discountAmount;
        if (finalAmount < 0) finalAmount = 0;

        return Result.Success(new ApplyPromotionResponse(discountAmount, finalAmount));
    }
}
