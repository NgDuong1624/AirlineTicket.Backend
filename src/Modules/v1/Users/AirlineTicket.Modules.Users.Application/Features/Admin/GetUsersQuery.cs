using MediatR;
using System.Collections.Generic;

namespace AirlineTicket.Modules.Users.Application.Features.Admin;

public record GetUsersQuery : IRequest<List<UserDto>>;
