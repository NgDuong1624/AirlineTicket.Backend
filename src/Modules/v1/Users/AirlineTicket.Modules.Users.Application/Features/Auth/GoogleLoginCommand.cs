using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using FluentValidation;

namespace AirlineTicket.Modules.Users.Application.Features.Auth;

public record GoogleLoginCommand(string IdToken) : ICommand<Result<TokenResponse>>;

public class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
{
    public GoogleLoginCommandValidator()
    {
        RuleFor(x => x.IdToken).NotEmpty();
    }
}
