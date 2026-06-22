using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using MediatR;
using System;

namespace AirlineTicket.Modules.Users.Application.Features.Admin;

public record AdminUpdateUserCommand(Guid Id, string FullName, string Phone, string Role) : ICommand<Result<Unit>>;
