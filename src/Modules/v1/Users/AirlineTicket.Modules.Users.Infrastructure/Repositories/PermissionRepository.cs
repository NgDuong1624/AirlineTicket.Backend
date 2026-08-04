using AirlineTicket.Modules.Users.Application.Repositories;
using AirlineTicket.Modules.Users.Domain.Entities;
using AirlineTicket.Modules.Users.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AirlineTicket.Modules.Users.Infrastructure.Repositories;

public class PermissionRepository : IPermissionRepository
{
    private readonly UserDbContext _context;

    public PermissionRepository(UserDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<Permission> Items, int TotalCount)> GetAllAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Permissions.AsNoTracking();
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .OrderBy(p => p.Id)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new Permission
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Description = p.Description
            })
            .ToListAsync(cancellationToken);
            
        return (items, totalCount);
    }

    public async Task<Permission?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Permissions
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new Permission
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Description = p.Description
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> CreateAsync(Permission permission, CancellationToken cancellationToken = default)
    {
        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync(cancellationToken);
        return permission.Id;
    }

    public async Task UpdateAsync(Permission permission, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Permissions.FirstOrDefaultAsync(p => p.Id == permission.Id, cancellationToken);
        if (existing is null) return;

        existing.Code = permission.Code;
        existing.Name = permission.Name;
        existing.Description = permission.Description;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Permissions.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (existing is null) return;

        _context.Permissions.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
