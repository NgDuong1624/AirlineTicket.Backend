using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Domain.Constants;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Users.Application.Repositories;
using AirlineTicket.Modules.Users.Application.Services;
using AirlineTicket.Modules.Users.Application.Features.Admin;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace AirlineTicket.Modules.Users.Application.Features.Auth;

public class LoginUserCommandHandler : ICommandHandler<LoginUserCommand, Result<LoginResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public LoginUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<Result<LoginResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Result.Failure<LoginResponse>(new Error(EndpointErrorCodes.UNAUTHORIZED, "Invalid email or password."));
        }

        if (!user.IsActive)
        {
            return Result.Failure<LoginResponse>(new Error(EndpointErrorCodes.UNAUTHORIZED, "Invalid email or password."));
        }

        var tokens = _jwtService.GenerateToken(user);
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);

        var userDto = new UserDto(
            user.Id,
            user.Email,
            user.FullName,
            user.Phone,
            user.Role.ToString(),
            (int)user.Role,
            user.AirlineId,
            user.AirlineName,
            user.AirlineLogoUrl,
            user.IsActive,
            user.CreatedAt.ToString("yyyy-MM-dd")
        );

        return Result.Success(new LoginResponse(tokens.AccessToken, tokens.RefreshToken, userDto));
    }
}
