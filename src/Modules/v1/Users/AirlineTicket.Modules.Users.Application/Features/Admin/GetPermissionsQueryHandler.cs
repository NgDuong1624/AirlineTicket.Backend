using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Users.Application.Repositories;
using AirlineTicket.Modules.Users.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Users.Application.Features.Admin;

public class GetPermissionsQueryHandler : IQueryHandler<GetPermissionsQuery, Result<IReadOnlyList<Permission>>>
{
    private readonly IPermissionRepository _repo;

    public GetPermissionsQueryHandler(IPermissionRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result<IReadOnlyList<Permission>>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        var items = await _repo.GetAllAsync(cancellationToken);
        return Result.Success(items);
    }
}