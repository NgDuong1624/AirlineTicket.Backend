using MediatR;
using System;

namespace AirlineTicket.Modules.Users.Application.Features.Admin;

public record GetUserByIdQuery(Guid Id) : IRequest<UserDto?>;
