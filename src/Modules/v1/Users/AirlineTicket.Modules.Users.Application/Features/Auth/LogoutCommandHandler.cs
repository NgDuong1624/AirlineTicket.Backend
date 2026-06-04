using AirlineTicket.Modules.Users.Application.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Users.Application.Features.Auth;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
{
    private readonly IUserRepository _userRepository;

    public LogoutCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (user != null)
        {
            // Revoke the refresh token
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _userRepository.UpdateAsync(user, cancellationToken);
        }

        return true;
    }
}
