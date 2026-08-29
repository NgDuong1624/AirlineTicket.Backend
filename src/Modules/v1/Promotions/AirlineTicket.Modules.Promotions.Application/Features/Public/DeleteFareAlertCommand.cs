using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using FluentValidation;

namespace AirlineTicket.Modules.Promotions.Application.Features.Public;

public record DeleteFareAlertCommand(Guid Id, Guid UserId) : ICommand<Result<bool>>;

public class DeleteFareAlertCommandValidator : AbstractValidator<DeleteFareAlertCommand>
{
    public DeleteFareAlertCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public sealed class DeleteFareAlertCommandHandler : ICommandHandler<DeleteFareAlertCommand, Result<bool>>
{
    private readonly IFareAlertRepository _repository;

    public DeleteFareAlertCommandHandler(IFareAlertRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> Handle(DeleteFareAlertCommand request, CancellationToken cancellationToken)
    {
        var deleted = await _repository.DeleteAsync(request.Id, request.UserId, cancellationToken);
        if (!deleted)
        {
            return Result.Failure<bool>(new Error("FareAlert.NotFound", "Fare alert not found."));
        }

        return Result.Success(true);
    }
}