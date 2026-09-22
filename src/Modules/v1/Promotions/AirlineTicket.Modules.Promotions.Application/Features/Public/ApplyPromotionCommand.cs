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
    private readonly IFlightAirlineLookup? _airlineLookup;

    public ApplyPromotionCommandHandler(
        IPromotionRepository promotionRepository,
        IFlightAirlineLookup? airlineLookup = null)
    {
        _promotionRepository = promotionRepository;
        _airlineLookup = airlineLookup;
    }

    public async Task<Result<ApplyPromotionResponse>> Handle(ApplyPromotionCommand request, CancellationToken cancellationToken)
    {
        var promo = await _promotionRepository.GetByCodeAsync(request.PromoCode, cancellationToken);
        if (promo == null)
        {
            return Result.Failure<ApplyPromotionResponse>(new Error("Promotion.NotFound", "Promotion code does not exist."));
        }

        var now = DateTime.UtcNow;
        if (now < promo.StartDate)
        {
            return Result.Failure<ApplyPromotionResponse>(new Error("Promotion.NotStarted", "Promotion code has not started yet."));
        }

        if (promo.EndDate < now)
        {
            return Result.Failure<ApplyPromotionResponse>(new Error("Promotion.Expired", "Promotion code is expired."));
        }

        if (promo.MaxUsage.HasValue && promo.CurrentUsage >= promo.MaxUsage.Value)
        {
            return Result.Failure<ApplyPromotionResponse>(new Error("Promotion.LimitReached", "Promotion code usage limit reached."));
        }

        if (promo.MinOrderValue.HasValue && request.OriginalAmount < promo.MinOrderValue.Value)
        {
            return Result.Failure<ApplyPromotionResponse>(new Error("Promotion.MinOrderNotMet", $"Minimum order amount for this coupon is {promo.MinOrderValue.Value:N0}."));
        }

        if (promo.AirlineId.HasValue && _airlineLookup != null && request.FlightId != Guid.Empty)
        {
            var flightAirlineId = await _airlineLookup.GetFlightAirlineIdAsync(request.FlightId, cancellationToken);
            if (flightAirlineId.HasValue && flightAirlineId.Value != promo.AirlineId.Value)
            {
                return Result.Failure<ApplyPromotionResponse>(new Error("Promotion.AirlineMismatch", "This coupon is only valid for flights operated by the issuing airline."));
            }
        }

        decimal discountAmount = 0;
        if (promo.DiscountType == "Percentage")
        {
            discountAmount = request.OriginalAmount * (promo.DiscountValue / 100m);
        }
        else if (promo.DiscountType == "FixedAmount")
        {
            discountAmount = promo.DiscountValue;
        }

        if (promo.MaxDiscountAmount.HasValue && discountAmount > promo.MaxDiscountAmount.Value)
        {
            discountAmount = promo.MaxDiscountAmount.Value;
        }

        var finalAmount = request.OriginalAmount - discountAmount;
        if (finalAmount < 0) finalAmount = 0;

        return Result.Success(new ApplyPromotionResponse(discountAmount, finalAmount));
    }
}
