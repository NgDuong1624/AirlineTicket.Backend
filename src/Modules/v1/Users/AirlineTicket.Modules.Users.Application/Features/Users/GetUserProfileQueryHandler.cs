using AirlineTicket.Modules.Users.Application.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Users.Application.Features.Admin;

namespace AirlineTicket.Modules.Users.Application.Features.Users;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserDto?>
{
    private readonly IUserRepository _userRepository;

    public GetUserProfileQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null) return null;

        return new UserDto(
            user.Id,
            user.Email,
            user.FullName,
            user.Phone,
            user.Role.ToString(),
            (int)user.Role,
            user.AirlineId,
            user.IsActive,
            user.CreatedAt.ToString("yyyy-MM-dd")
        );
    }
}
