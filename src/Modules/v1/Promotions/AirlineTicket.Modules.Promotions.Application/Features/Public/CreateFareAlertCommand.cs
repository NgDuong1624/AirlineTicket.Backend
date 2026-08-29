using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using AirlineTicket.Modules.Promotions.Domain.Entities;
using FluentValidation;

namespace AirlineTicket.Modules.Promotions.Application.Features.Public;

public record CreateFareAlertCommand(
    Guid UserId,
    Guid OriginAirportId,
    Guid DestinationAirportId,
    DateOnly DepartureDate,
    DateOnly? ReturnDate,
    decimal TargetPrice,
    decimal CurrentLowestPrice,
    string Currency = "VND"
) : ICommand<Result<Guid>>;

public class CreateFareAlertCommandValidator : AbstractValidator<CreateFareAlertCommand>
{
    public CreateFareAlertCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.OriginAirportId).NotEmpty();
        RuleFor(x => x.DestinationAirportId).NotEmpty();
        RuleFor(x => x.OriginAirportId)
            .NotEqual(x => x.DestinationAirportId)
            .WithMessage("Origin and destination airports must be different.");

        RuleFor(x => x.DepartureDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Departure date cannot be in the past.");

        RuleFor(x => x.ReturnDate)
            .Must((cmd, returnDate) => !returnDate.HasValue || returnDate.Value >= cmd.DepartureDate)
            .WithMessage("Return date cannot be earlier than departure date.");

        RuleFor(x => x.TargetPrice)
            .GreaterThan(0)
            .WithMessage("Target price must be greater than zero.");
    }
}

public sealed class CreateFareAlertCommandHandler : ICommandHandler<CreateFareAlertCommand, Result<Guid>>
{
    private const int MaxActiveAlertsPerUser = 10;
    private readonly IFareAlertRepository _repository;

    public CreateFareAlertCommandHandler(IFareAlertRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(CreateFareAlertCommand request, CancellationToken cancellationToken)
    {
        var existingAlert = await _repository.GetUserAlertForRouteAsync(
            request.UserId,
            request.OriginAirportId,
            request.DestinationAirportId,
            request.DepartureDate,
            cancellationToken);

        if (existingAlert != null)
        {
            return Result.Failure<Guid>(new Error("FareAlert.Duplicate", "A fare alert already exists for this route and departure date."));
        }

        var activeCount = await _repository.CountActiveByUserIdAsync(request.UserId, cancellationToken);
        if (activeCount >= MaxActiveAlertsPerUser)
        {
            return Result.Failure<Guid>(new Error("FareAlert.LimitExceeded", $"Maximum of {MaxActiveAlertsPerUser} active fare alerts allowed."));
        }

        var alert = new FareAlert
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            OriginAirportId = request.OriginAirportId,
            DestinationAirportId = request.DestinationAirportId,
            DepartureDate = request.DepartureDate,
            ReturnDate = request.ReturnDate,
            TargetPrice = request.TargetPrice,
            CurrentLowestPrice = request.CurrentLowestPrice,
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "VND" : request.Currency,
            IsActive = true,
            LastCheckedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var id = await _repository.CreateAsync(alert, cancellationToken);
        return Result.Success(id);
    }
}
