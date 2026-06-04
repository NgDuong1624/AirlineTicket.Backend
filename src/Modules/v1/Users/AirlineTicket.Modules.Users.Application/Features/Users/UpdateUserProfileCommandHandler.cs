using AirlineTicket.Modules.Users.Application.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Users.Application.Features.Users;

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, bool>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserProfileCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            throw new UnauthorizedAccessException("User not found.");

        user.FullName = request.FullName;
        user.Phone = request.Phone;

        await _userRepository.UpdateAsync(user, cancellationToken);
        return true;
    }
}
