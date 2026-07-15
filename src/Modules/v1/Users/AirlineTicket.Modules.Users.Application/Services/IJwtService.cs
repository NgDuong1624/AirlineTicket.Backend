using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Users.Domain.Entities;

namespace AirlineTicket.Modules.Users.Application.Services;

public interface IJwtService
{
    TokenResponse GenerateToken(User user);
}
