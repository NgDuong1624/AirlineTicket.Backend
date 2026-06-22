using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using MediatR;
using FluentValidation;

namespace AirlineTicket.Modules.Users.Application.Features.Auth;

public record LogoutCommand(string RefreshToken) : ICommand<Result<Unit>>;

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
