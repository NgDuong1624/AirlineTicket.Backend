using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;

namespace AirlineTicket.Modules.Users.Application.Features.Auth;

public record RefreshTokenCommand(string RefreshToken) : ICommand<Result<TokenResponse>>;
