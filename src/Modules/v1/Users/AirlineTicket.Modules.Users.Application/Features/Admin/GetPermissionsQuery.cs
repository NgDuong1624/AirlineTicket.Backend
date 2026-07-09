using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Users.Domain.Entities;

namespace AirlineTicket.Modules.Users.Application.Features.Admin;

public record GetPermissionsQuery() : IQuery<Result<IReadOnlyList<Permission>>>;