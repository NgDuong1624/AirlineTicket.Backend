using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Users.Domain.Entities;

namespace AirlineTicket.Modules.Users.Application.Repositories;

public interface IUserPermissionScopeRepository
{
    Task<IReadOnlyList<UserPermissionScope>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid userId, int permissionId, CancellationToken cancellationToken = default);
    Task AddAsync(UserPermissionScope scope, CancellationToken cancellationToken = default);
    Task RemoveAsync(Guid userId, int permissionId, CancellationToken cancellationToken = default);
}