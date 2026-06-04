using MediatR;
using FluentValidation;

namespace AirlineTicket.Modules.Users.Application.Features.Auth;

public record LoginUserCommand(string Email, string Password) : IRequest<string>;

public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
