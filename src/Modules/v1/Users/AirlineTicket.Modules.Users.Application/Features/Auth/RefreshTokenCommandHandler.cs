using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Users.Application.Repositories;
using AirlineTicket.Modules.Users.Application.Services;
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

        var accessToken = _jwtService.GenerateToken(user);

        // Return the same refreshToken (no rotation) — only accessToken is renewed.
        return Result.Success(new LoginResponse(accessToken, request.RefreshToken));
    }
}
