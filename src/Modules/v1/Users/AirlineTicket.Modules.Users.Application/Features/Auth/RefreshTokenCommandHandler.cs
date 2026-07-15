using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Users.Application.Repositories;
using AirlineTicket.Modules.Users.Application.Services;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Users.Application.Features.Auth;

public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, Result<TokenResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public RefreshTokenCommandHandler(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<Result<TokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return Result.Failure<TokenResponse>(new Error("INVALID_REFRESH_TOKEN", "Invalid or expired refresh token."));
        }

        // GenerateToken rotates the refresh token (sets new RefreshToken + Expiry on user)
        var tokens = _jwtService.GenerateToken(user);

        // Persist rotated refresh token
        await _userRepository.UpdateAsync(user, cancellationToken);

        return Result.Success(tokens);
    }
}
