using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using MediatR;

namespace AirlineTicket.Modules.Promotions.Application.Features.Admin;

public record CreatePromotionCommand(string Name, string PromoCode, string DiscountType, decimal DiscountValue, int MaxUsage, DateTime StartDate, DateTime EndDate) : ICommand<Result<Guid>>;

public class CreatePromotionCommandHandler : ICommandHandler<CreatePromotionCommand, Result<Guid>>
{
    private readonly IPromotionRepository _promotionRepository;

    public CreatePromotionCommandHandler(IPromotionRepository promotionRepository)
    {
        _promotionRepository = promotionRepository;
    }

    public async Task<Result<Guid>> Handle(CreatePromotionCommand request, CancellationToken cancellationToken)
    {
        var promo = new PromotionDto
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            PromoCode = request.PromoCode,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            MaxUsage = request.MaxUsage,
            CurrentUsage = 0,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        var id = await _promotionRepository.CreateAsync(promo, cancellationToken);
        return Result.Success(id);
    }
}
