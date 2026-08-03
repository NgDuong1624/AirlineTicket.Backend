using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Users.Application.Repositories;
using MediatR;

namespace AirlineTicket.Modules.Users.Application.Features.Commands;

public class UpdateLanguageCommandHandler : IRequestHandler<UpdateLanguageCommand, Result>
{
    private readonly IUserRepository _userRepository;

    public UpdateLanguageCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result> Handle(UpdateLanguageCommand request, CancellationToken cancellationToken)
    {
        var validLanguages = new[] { "en", "vi", "zh", "ja", "ko", "fr" };
        if (!Array.Exists(validLanguages, lang => lang.Equals(request.Language, StringComparison.OrdinalIgnoreCase)))
        {
            return Result.Failure(new Error("Language.Invalid", "Invalid language code."));
        }

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return Result.Failure(new Error("User.NotFound", "User not found."));
        }

        user.LanguagePreference = request.Language.ToLowerInvariant();
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);

        return Result.Success();
    }
}
