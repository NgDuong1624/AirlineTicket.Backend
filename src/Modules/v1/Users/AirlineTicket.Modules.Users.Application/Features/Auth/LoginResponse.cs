namespace AirlineTicket.Modules.Users.Application.Features.Auth;

using AirlineTicket.Modules.Users.Application.Features.Admin;

public sealed record LoginResponse(string AccessToken, string RefreshToken, UserDto User);
