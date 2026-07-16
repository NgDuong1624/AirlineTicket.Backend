namespace AirlineTicket.Modules.Users.Application.Features.Auth;

public sealed record LoginResponse(string AccessToken, string RefreshToken);
