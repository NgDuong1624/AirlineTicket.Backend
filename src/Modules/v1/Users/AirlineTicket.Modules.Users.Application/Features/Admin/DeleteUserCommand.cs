using MediatR;
using System;

namespace AirlineTicket.Modules.Users.Application.Features.Admin;

public record DeleteUserCommand(Guid Id) : IRequest<bool>;
