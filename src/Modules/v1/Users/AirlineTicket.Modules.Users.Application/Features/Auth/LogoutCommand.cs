using MediatR;
using FluentValidation;

namespace AirlineTicket.Modules.Users.Application.Features.Auth;

public record LogoutCommand(string RefreshToken) : IRequest<bool>;

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
