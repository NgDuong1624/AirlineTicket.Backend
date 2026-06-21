using AirlineTicket.Modules.Users.Application.Repositories;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Users.Application.Features.Admin;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        
        return users.Select(user => new UserDto(
            user.Id,
            user.Email,
            user.FullName,
            user.Phone,

            user.Role.ToString(),
            (int)user.Role,
            user.AirlineId,
            user.IsActive,
            user.CreatedAt.ToString("yyyy-MM-dd")
        )).ToList();
    }
}
