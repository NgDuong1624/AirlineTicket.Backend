using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Users.Application.Repositories;
using AirlineTicket.Modules.Users.Application.Services;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Users.Application.Features.Auth;

public class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand, Result<Unit>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<Unit>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return Result.Failure<Unit>(new Error("User.NotFound", "User not found."));
        }

        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            return Result.Failure<Unit>(new Error("Auth.IncorrectPassword", "Incorrect email or password."));
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);

        await _userRepository.UpdateAsync(user, cancellationToken);

        return Result.Success(Unit.Value);
    }
}
