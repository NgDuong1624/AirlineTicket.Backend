using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Users.Application.Repositories;
using AirlineTicket.Modules.Users.Application.Services;
using AirlineTicket.Modules.Users.Domain.Enums;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Users.Application.Features.Admin;

public class AdminUpdateUserCommandHandler : ICommandHandler<AdminUpdateUserCommand, Result<Unit>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public AdminUpdateUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<Unit>> Handle(AdminUpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user == null)
            return Result.Failure<Unit>(new Error("User.NotFound", "User not found."));

        user.FullName = request.FullName;
        user.Phone = request.Phone;
        
        if (request.RoleId.HasValue) 
            user.Role = request.RoleId.Value;
            
        if (request.IsActive.HasValue) 
            user.IsActive = request.IsActive.Value;
            
        if (!string.IsNullOrEmpty(request.Password))
            user.PasswordHash = _passwordHasher.HashPassword(request.Password);
            
        user.AirlineId = request.AirlineId;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        return Result.Success(Unit.Value);
    }
}
