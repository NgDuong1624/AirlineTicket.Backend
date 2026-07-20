using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
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

    public RegisterUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (!await _userRepository.IsEmailUniqueAsync(request.Email, cancellationToken))
        {
            return Result.Failure<Guid>(new Error("EMAIL_ALREADY_EXISTS", "Email already exists."));
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FullName = request.FullName,
            Phone = request.Phone,
            Role = (int)UserRole.Customer
        };

        await _userRepository.AddAsync(user, cancellationToken);

        return Result.Success(user.Id);
    }
}
