using System;

namespace AirlineTicket.Modules.Users.Application.Features.Admin;

public record UserDto(Guid Id, string Email, string FullName, string? Phone, string Role, int RoleId);