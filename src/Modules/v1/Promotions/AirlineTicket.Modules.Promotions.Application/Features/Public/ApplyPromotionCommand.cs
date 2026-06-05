using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using MediatR;

namespace AirlineTicket.Modules.Promotions.Application.Features.Public;

public record ApplyPromotionCommand(string PromoCode, Guid FlightId, decimal OriginalAmount) : IRequest<object>;

public class ApplyPromotionCommandHandler : IRequestHandler<ApplyPromotionCommand, object>
{
    private readonly IPromotionRepository _promotionRepository;

    public ApplyPromotionCommandHandler(IPromotionRepository promotionRepository)
    {
        _promotionRepository = promotionRepository;
    }

    public async Task<object> Handle(ApplyPromotionCommand request, CancellationToken cancellationToken)
    {
        var promo = await _promotionRepository.GetByCodeAsync(request.PromoCode, cancellationToken);
        if (promo == null || promo.EndDate < DateTime.UtcNow || promo.CurrentUsage >= promo.MaxUsage)
        {
            throw new Exception("Mã giảm giá không hợp lệ hoặc đã hết hạn.");
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

        return new { DiscountAmount = discountAmount, FinalAmount = finalAmount };
    }
}
