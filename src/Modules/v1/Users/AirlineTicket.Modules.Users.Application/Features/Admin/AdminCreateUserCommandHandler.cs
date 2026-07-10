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

namespace AirlineTicket.Modules.Users.Application.Features.Admin;

public class AdminCreateUserCommandHandler : ICommandHandler<AdminCreateUserCommand, Result<Guid>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public AdminCreateUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<Guid>> Handle(AdminCreateUserCommand request, CancellationToken cancellationToken)
    {
        if (!await _userRepository.IsEmailUniqueAsync(request.Email, cancellationToken))
            return Result.Failure<Guid>(new Error("BAD_REQUEST", "Email already exists"));

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FullName = request.FullName,
            Phone = request.Phone,
            Role = request.RoleId ?? (int)Domain.Enums.UserRole.Customer,
            IsActive = request.IsActive ?? true,
            PasswordHash = _passwordHasher.HashPassword(string.IsNullOrEmpty(request.Password) ? "ChangeMe123!" : request.Password),
            AirlineId = request.AirlineId
        };

        await _userRepository.AddAsync(user, cancellationToken);
        return Result.Success(user.Id);
    }
}