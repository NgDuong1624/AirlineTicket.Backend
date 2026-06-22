using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Users.Application.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Users.Application.Features.Users;

public class UpdateUserProfileCommandHandler : ICommandHandler<UpdateUserProfileCommand, Result<Unit>>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserProfileCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<Unit>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            return Result.Failure<Unit>(new Error("USER_NOT_FOUND", "User not found."));

        user.FullName = request.FullName;
        user.Phone = request.Phone;

        await _userRepository.UpdateAsync(user, cancellationToken);
        return Result.Success(Unit.Value);
    }
}
