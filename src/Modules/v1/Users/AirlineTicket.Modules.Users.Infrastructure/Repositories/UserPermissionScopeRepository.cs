using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Users.Application.Repositories;
using AirlineTicket.Modules.Users.Domain.Entities;
using AirlineTicket.Modules.Users.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AirlineTicket.Modules.Users.Infrastructure.Repositories;

public class UserPermissionScopeRepository : IUserPermissionScopeRepository
{
    private readonly UserDbContext _context;

    public UserPermissionScopeRepository(UserDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<UserPermissionScope>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserPermissionScopes
            .Include(x => x.Permission)
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid userId, int permissionId, CancellationToken cancellationToken = default)
    {
        return await _context.UserPermissionScopes.AnyAsync(x => x.UserId == userId && x.PermissionId == permissionId, cancellationToken);
    }

    public async Task AddAsync(UserPermissionScope scope, CancellationToken cancellationToken = default)
    {
        _context.UserPermissionScopes.Add(scope);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(Guid userId, int permissionId, CancellationToken cancellationToken = default)
    {
        var scope = await _context.UserPermissionScopes
            .FirstOrDefaultAsync(x => x.UserId == userId && x.PermissionId == permissionId, cancellationToken);
        if (scope != null)
        {
            _context.UserPermissionScopes.Remove(scope);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}