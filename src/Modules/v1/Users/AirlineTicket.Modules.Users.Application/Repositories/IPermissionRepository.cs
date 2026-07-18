using AirlineTicket.Modules.Users.Domain.Entities;

namespace AirlineTicket.Modules.Users.Application.Repositories;

public interface IPermissionRepository
{
    Task<(IReadOnlyList<Permission> Items, int TotalCount)> GetAllAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default);
    Task<Permission?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(Permission permission, CancellationToken cancellationToken = default);
    Task UpdateAsync(Permission permission, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
