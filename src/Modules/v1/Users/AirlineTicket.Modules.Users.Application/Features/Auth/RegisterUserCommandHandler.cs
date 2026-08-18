using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.BuildingBlocks.Application.Localization;
using AirlineTicket.Modules.Users.Application.Repositories;
using AirlineTicket.Modules.Users.Application.Services;
using AirlineTicket.Modules.Users.Domain.Entities;
using AirlineTicket.Modules.Users.Domain.Enums;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Users.Application.Features.Auth;

public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, Result<Guid>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILanguageResolver _languageResolver;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ILanguageResolver languageResolver)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _languageResolver = languageResolver;
    }

    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (!await _userRepository.IsEmailUniqueAsync(request.Email, cancellationToken))
        {
            return Result.Failure<Guid>(new Error("EMAIL_ALREADY_EXISTS", "Email already exists."));
        }

        var resolvedLanguage = !string.IsNullOrWhiteSpace(request.LanguagePreference)
            ? _languageResolver.NormalizeCulture(request.LanguagePreference)
            : _languageResolver.ResolveLanguage();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FullName = request.FullName,
            Phone = request.Phone,
            LanguagePreference = string.IsNullOrWhiteSpace(resolvedLanguage) ? "en" : resolvedLanguage,
            Role = (int)UserRole.Customer
        };

        await _userRepository.AddAsync(user, cancellationToken);

        return Result.Success(user.Id);
    }
}
