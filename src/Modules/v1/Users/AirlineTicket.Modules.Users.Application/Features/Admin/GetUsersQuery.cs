using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using MediatR;
using System.Collections.Generic;

namespace AirlineTicket.Modules.Users.Application.Features.Admin;

public record GetUsersQuery : IQuery<Result<List<UserDto>>>;
