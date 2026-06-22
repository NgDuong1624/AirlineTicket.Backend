using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Users.Application.Features.Admin;
using MediatR;
using System;

namespace AirlineTicket.Modules.Users.Application.Features.Users;

public record GetUserProfileQuery(Guid UserId) : IQuery<Result<UserDto?>>;
