using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using MediatR;
using System;

namespace AirlineTicket.Modules.Users.Application.Features.Admin;

public record AdminResetPasswordCommand(Guid Id, string NewPassword) : ICommand<Result<Unit>>;
