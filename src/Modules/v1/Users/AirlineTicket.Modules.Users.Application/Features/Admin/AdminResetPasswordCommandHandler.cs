using AirlineTicket.Modules.Users.Application.Repositories;
using AirlineTicket.Modules.Users.Application.Services;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Users.Application.Features.Admin;

public class AdminResetPasswordCommandHandler : IRequestHandler<AdminResetPasswordCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public AdminResetPasswordCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<bool> Handle(AdminResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user == null)
            throw new UnauthorizedAccessException("User not found.");

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;

        await _userRepository.UpdateAsync(user, cancellationToken);
        return true;
    }
}
