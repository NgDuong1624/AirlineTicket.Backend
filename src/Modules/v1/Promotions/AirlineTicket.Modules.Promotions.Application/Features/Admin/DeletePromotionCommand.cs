using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using MediatR;

namespace AirlineTicket.Modules.Promotions.Application.Features.Admin;

public record DeletePromotionCommand(Guid Id) : ICommand<Unit>;

internal sealed class DeletePromotionCommandHandler : ICommandHandler<DeletePromotionCommand, Unit>
{
    private readonly IPromotionRepository _promotionRepository;
    public DeletePromotionCommandHandler(IPromotionRepository promotionRepository) => _promotionRepository = promotionRepository;
    public async Task<Unit> Handle(DeletePromotionCommand request, CancellationToken cancellationToken)
    {
        await _promotionRepository.DeleteAsync(request.Id, cancellationToken);
        return Unit.Value;
    }
}
