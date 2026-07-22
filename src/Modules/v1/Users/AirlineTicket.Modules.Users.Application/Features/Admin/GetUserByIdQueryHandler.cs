using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Users.Application.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Users.Application.Features.Admin;

public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, Result<UserDto?>>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto?>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user == null)
            return Result.Failure<UserDto?>(new Error("USER_NOT_FOUND", "User not found."));

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

        return Result.Success<UserDto?>(userDto);
    }
}
