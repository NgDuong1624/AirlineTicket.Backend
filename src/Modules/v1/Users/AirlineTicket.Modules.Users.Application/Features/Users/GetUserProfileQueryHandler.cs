using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Users.Application.Repositories;
using AirlineTicket.Modules.Users.Application.Contracts;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Users.Application.Features.Admin;

namespace AirlineTicket.Modules.Users.Application.Features.Users;

public class GetUserProfileQueryHandler : IQueryHandler<GetUserProfileQuery, Result<UserDto?>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserBookingStatsProvider? _bookingStatsProvider;

    public GetUserProfileQueryHandler(
        IUserRepository userRepository,
        IUserBookingStatsProvider? bookingStatsProvider = null)
    {
        _userRepository = userRepository;
        _bookingStatsProvider = bookingStatsProvider;
    }

    public async Task<Result<UserDto?>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            return Result.Failure<UserDto?>(new Error("USER_NOT_FOUND", "User not found."));

        UserBookingStatsDto? bookingStats = null;
        if (_bookingStatsProvider != null)
        {
            try
            {
                bookingStats = await _bookingStatsProvider.GetStatsByUserIdAsync(request.UserId, cancellationToken);
            }
            catch
            {
                // Fallback gracefully if stats cannot be retrieved
            }
        }

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
            user.CreatedAt.ToString("yyyy-MM-dd"),
            bookingStats
        );

        return Result.Success<UserDto?>(userDto);
    }
}
