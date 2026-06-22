using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using MediatR;
using System;

namespace AirlineTicket.Modules.Users.Application.Features.Admin;

public record DeleteUserCommand(Guid Id) : ICommand<Result<Unit>>;
