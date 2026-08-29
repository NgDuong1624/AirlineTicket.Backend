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

        RuleFor(x => x.TargetPrice)
            .GreaterThan(0)
            .When(x => x.TargetPrice.HasValue)
            .WithMessage("Target price must be greater than zero.");
    }
}

public sealed class UpdateFareAlertCommandHandler : ICommandHandler<UpdateFareAlertCommand, Result<bool>>
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