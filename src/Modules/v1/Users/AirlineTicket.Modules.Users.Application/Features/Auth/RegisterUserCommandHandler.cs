using AirlineTicket.Modules.Users.Application.Repositories;
using AirlineTicket.Modules.Users.Application.Services;
using AirlineTicket.Modules.Users.Domain.Entities;
using AirlineTicket.Modules.Users.Domain.Enums;
using MediatR;

namespace AirlineTicket.Modules.Users.Application.Features.Auth;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (!await _userRepository.IsEmailUniqueAsync(request.Email, cancellationToken))
        {
            throw new Exception("Email already exists");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FullName = request.FullName,
            Phone = request.Phone,
            Role = UserRole.Customer
        };

        await _userRepository.AddAsync(user, cancellationToken);

        return user.Id;
    }
}
