using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using FluentValidation;

namespace AirlineTicket.Modules.Promotions.Application.Features.Public;

public record UpdateFareAlertCommand(
    Guid Id,
    Guid UserId,
    decimal? TargetPrice,
    bool? IsActive
) : ICommand<Result<bool>>;

public class UpdateFareAlertCommandValidator : AbstractValidator<UpdateFareAlertCommand>
{
    public UpdateFareAlertCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();

        When(x => x.TargetPrice.HasValue, () =>
        {
            RuleFor(x => x.TargetPrice!.Value)
                .GreaterThan(0)
                .WithMessage("Target price must be greater than zero.");
        });
    }
}

internal sealed class UpdateFareAlertCommandHandler : ICommandHandler<UpdateFareAlertCommand, Result<bool>>
{
    private readonly IFareAlertRepository _repository;

    public UpdateFareAlertCommandHandler(IFareAlertRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> Handle(UpdateFareAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (alert == null || alert.UserId != request.UserId)
        {
            return Result.Failure<bool>(new Error("FareAlert.NotFound", "Fare alert not found."));
        }

        if (request.TargetPrice.HasValue)
        {
            alert.TargetPrice = request.TargetPrice.Value;
        }

        if (request.IsActive.HasValue)
        {
            alert.IsActive = request.IsActive.Value;
        }

        var updated = await _repository.UpdateAsync(alert, cancellationToken);
        return Result.Success(updated);
    }
}