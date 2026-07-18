using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Users.Application.Repositories;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Users.Application.Features.Admin;

public class GetUsersQueryHandler : IQueryHandler<GetUsersQuery, Result<PagedResult<UserDto>>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<PagedResult<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var (users, totalCount) = await _userRepository.GetAllAsync(request.PageIndex, request.PageSize, cancellationToken);

        var userDtos = users.Select(user => new UserDto(
            user.Id,
            user.Email,
            user.FullName,
            user.Phone,
            user.Role.ToString(),
            (int)user.Role,
            user.AirlineId,
            user.AirlineName,
            user.IsActive,
            user.CreatedAt.ToString("yyyy-MM-dd")
        )).ToList();

        return Result.Success(PagedResult<UserDto>.Success(userDtos, request.PageIndex, request.PageSize, totalCount));
    }
}
