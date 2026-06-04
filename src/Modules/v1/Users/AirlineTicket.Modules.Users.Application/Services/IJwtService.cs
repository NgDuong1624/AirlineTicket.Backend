using AirlineTicket.Modules.Users.Domain.Entities;

namespace AirlineTicket.Modules.Users.Application.Services;

public interface IJwtService
{
    string GenerateToken(User user);
}
