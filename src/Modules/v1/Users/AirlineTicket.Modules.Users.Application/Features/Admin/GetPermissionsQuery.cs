using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Users.Domain.Entities;

namespace AirlineTicket.Modules.Users.Application.Features.Admin;

public record GetPermissionsQuery(int PageIndex = 1, int PageSize = 10) : IQuery<Result<PagedResult<Permission>>>;