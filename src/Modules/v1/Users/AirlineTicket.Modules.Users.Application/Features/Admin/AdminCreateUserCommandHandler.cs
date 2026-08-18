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

namespace AirlineTicket.Modules.Users.Application.Features.Admin;

public class AdminCreateUserCommandHandler : ICommandHandler<AdminCreateUserCommand, Result<Guid>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILanguageResolver _languageResolver;

    public AdminCreateUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ILanguageResolver languageResolver)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _languageResolver = languageResolver;
    }

    public async Task<Result<Guid>> Handle(AdminCreateUserCommand request, CancellationToken cancellationToken)
    {
        if (!await _userRepository.IsEmailUniqueAsync(request.Email, cancellationToken))
            return Result.Failure<Guid>(new Error("BAD_REQUEST", "Email already exists"));

        var resolvedLanguage = !string.IsNullOrWhiteSpace(request.LanguagePreference)
            ? _languageResolver.NormalizeCulture(request.LanguagePreference)
            : _languageResolver.ResolveLanguage();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FullName = request.FullName,
            Phone = request.Phone,
            Role = request.RoleId ?? (int)Domain.Enums.UserRole.Customer,
            IsActive = request.IsActive ?? true,
            PasswordHash = _passwordHasher.HashPassword(string.IsNullOrEmpty(request.Password) ? "ChangeMe123!" : request.Password),
            AirlineId = request.AirlineId,
            LanguagePreference = string.IsNullOrWhiteSpace(resolvedLanguage) ? "en" : resolvedLanguage
        };

        await _userRepository.AddAsync(user, cancellationToken);
        return Result.Success(user.Id);
    }
}