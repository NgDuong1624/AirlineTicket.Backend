using MediatR;
using FluentValidation;

namespace AirlineTicket.Modules.Users.Application.Features.Auth;

public record RefreshTokenCommand(string RefreshToken) : IRequest<string>;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
