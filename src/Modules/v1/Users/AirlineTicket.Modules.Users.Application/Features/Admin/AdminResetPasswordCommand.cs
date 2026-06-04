using MediatR;
using System;

namespace AirlineTicket.Modules.Users.Application.Features.Admin;

public record AdminResetPasswordCommand(Guid Id, string NewPassword) : IRequest<bool>;
