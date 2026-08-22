using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Users.Application.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Users.Application.Features.Admin;

namespace AirlineTicket.Modules.Users.Application.Features.Users;

public class GetUserProfileQueryHandler : IQueryHandler<GetUserProfileQuery, Result<UserDto?>>
{
    private readonly IUserRepository _userRepository;

    public GetUserProfileQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto?>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            return Result.Failure<UserDto?>(new Error("User.NotFound", "User not found."));

        var userDto = new UserDto(
            user.Id,
            user.Email,
            user.FullName,
            user.Phone,
            ((Domain.Enums.UserRole)user.Role).ToString(),
            user.Role,
            user.AirlineId,
            user.AirlineName,
            user.AirlineLogoUrl,
            user.LanguagePreference,
            user.IsActive,
            user.CreatedAt.ToString("yyyy-MM-dd")
        );

        return Result.Success<UserDto?>(userDto);
    }
}
