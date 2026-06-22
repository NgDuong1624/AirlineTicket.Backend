using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using MediatR;

namespace AirlineTicket.Modules.Promotions.Application.Features.Admin;

public record UpdatePromotionCommand(Guid Id, string Name, decimal DiscountValue, DateTime EndDate) : ICommand<Result<Unit>>;

internal sealed class UpdatePromotionCommandHandler : ICommandHandler<UpdatePromotionCommand, Result<Unit>>
{
    private readonly IPromotionRepository _repo;
    public UpdatePromotionCommandHandler(IPromotionRepository repo) => _repo = repo;
    public async Task<Result<Unit>> Handle(UpdatePromotionCommand request, CancellationToken cancellationToken)
    {
        await _repo.UpdateAsync(new PromotionDto { Id = request.Id, Name = request.Name, DiscountValue = request.DiscountValue, EndDate = request.EndDate }, cancellationToken);
        return Result.Success(Unit.Value);
    }
}
