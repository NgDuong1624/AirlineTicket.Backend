using MediatR;
using System;

namespace AirlineTicket.Modules.Users.Application.Features.Users;

public record GetUserProfileQuery(Guid UserId) : IRequest<AirlineTicket.Modules.Users.Application.Features.Admin.UserDto?>;
