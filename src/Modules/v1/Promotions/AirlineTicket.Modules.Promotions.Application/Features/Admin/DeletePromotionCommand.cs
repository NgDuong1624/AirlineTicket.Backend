using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using AirlineTicket.BuildingBlocks.Caching;
using AirlineTicket.Modules.Promotions.Application.Features.Public;
using MediatR;

namespace AirlineTicket.Modules.Promotions.Application.Features.Admin;

public record DeletePromotionCommand(Guid Id) : ICommand<Result<Unit>>;

internal sealed class DeletePromotionCommandHandler : ICommandHandler<DeletePromotionCommand, Result<Unit>>
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly ICacheService _cacheService;

    public DeletePromotionCommandHandler(IPromotionRepository promotionRepository, ICacheService cacheService)
    {
        _promotionRepository = promotionRepository;
        _cacheService = cacheService;
    }
    
    public async Task<Result<Unit>> Handle(DeletePromotionCommand request, CancellationToken cancellationToken)
    {
        await _promotionRepository.DeleteAsync(request.Id, cancellationToken);
        
        await _cacheService.RemoveAsync(CacheKeyBuilder.ForQuery<GetActivePromotionsQuery>("all"), cancellationToken);
        
        return Result.Success(Unit.Value);
    }
}
