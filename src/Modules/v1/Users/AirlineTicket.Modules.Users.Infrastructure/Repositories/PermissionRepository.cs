using AirlineTicket.Modules.Users.Application.Repositories;
using AirlineTicket.Modules.Users.Domain.Entities;
using AirlineTicket.Modules.Users.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Dapper;

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
        var connection = _context.Database.GetDbConnection();
        const string sql = @"
            SELECT * FROM dbo.Permissions
            ORDER BY Id
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
        const string countSql = "SELECT COUNT(*) FROM dbo.Permissions";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql);
        var result = await connection.QueryAsync<Permission>(sql, new { Offset = (pageIndex - 1) * pageSize, PageSize = pageSize });
        return (result.ToList(), totalCount);
    }

    public async Task<Permission?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        return await connection.QueryFirstOrDefaultAsync<Permission>(
            "SELECT * FROM dbo.Permissions WHERE Id = @Id", new { Id = id });
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
