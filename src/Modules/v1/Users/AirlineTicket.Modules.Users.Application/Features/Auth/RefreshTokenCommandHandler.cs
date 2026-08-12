using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Users.Application.Repositories;
using AirlineTicket.Modules.Users.Application.Services;
using AirlineTicket.Modules.Users.Application.Features.Admin;
using MediatR;

namespace AirlineTicket.Modules.Users.Application.Features.Auth;

public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public RefreshTokenCommandHandler(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<Result<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return Result.Failure<LoginResponse>(new Error("INVALID_REFRESH_TOKEN", "Invalid or expired refresh token."));
        }

        var tokens = _jwtService.GenerateToken(user);
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
