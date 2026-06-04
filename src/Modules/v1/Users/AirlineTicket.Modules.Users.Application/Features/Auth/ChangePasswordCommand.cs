using MediatR;
using FluentValidation;

namespace AirlineTicket.Modules.Users.Application.Features.Auth;

public record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword) : IRequest<bool>;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6);
    }
}
