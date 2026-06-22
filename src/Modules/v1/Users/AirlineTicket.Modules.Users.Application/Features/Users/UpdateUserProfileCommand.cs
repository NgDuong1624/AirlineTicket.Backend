using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using MediatR;
using System;

namespace AirlineTicket.Modules.Users.Application.Features.Users;

public record UpdateUserProfileCommand(Guid UserId, string FullName, string Phone) : ICommand<Result<Unit>>;
