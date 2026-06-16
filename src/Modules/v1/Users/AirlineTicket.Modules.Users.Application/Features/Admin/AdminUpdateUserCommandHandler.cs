using AirlineTicket.Modules.Users.Application.Repositories;
using AirlineTicket.Modules.Users.Domain.Enums;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Users.Application.Features.Admin;

public class AdminUpdateUserCommandHandler : IRequestHandler<AdminUpdateUserCommand, bool>
{
    private readonly IUserRepository _userRepository;

    public AdminUpdateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> Handle(AdminUpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user == null)
            throw new UnauthorizedAccessException("User not found.");

        user.FullName = request.FullName;
        user.Phone = request.Phone;

        if (Enum.TryParse<AirlineTicket.Modules.Users.Domain.Enums.UserRole>(request.Role, true, out var roleEnum))
        {
            user.Role = roleEnum;
        }

        await _userRepository.UpdateAsync(user, cancellationToken);
        return true;
    }
}
