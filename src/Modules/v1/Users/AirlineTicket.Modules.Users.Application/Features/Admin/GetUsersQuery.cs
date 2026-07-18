using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using MediatR;
using System.Collections.Generic;

namespace AirlineTicket.Modules.Users.Application.Features.Admin;

public record GetUsersQuery(int PageIndex = 1, int PageSize = 10) : IQuery<Result<PagedResult<UserDto>>>;
