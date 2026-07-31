using AirlineTicket.Modules.Users.Application.Repositories;
using AirlineTicket.Modules.Users.Domain.Entities;
using AirlineTicket.Modules.Users.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Dapper;

namespace AirlineTicket.Modules.Users.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserDbContext _context;

    public UserRepository(UserDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<User> Items, int TotalCount)> GetAllAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        const string countSql = "SELECT COUNT(*) FROM users.users";
        const string sql = @"
            SELECT u.*, a.name as AirlineName, a.logo_url as AirlineLogoUrl 
            FROM users.users u 
            LEFT JOIN flights.airlines a ON u.airline_id = a.id
            ORDER BY u.created_at DESC
            OFFSET @Offset LIMIT @PageSize";

        var totalCount = await connection.ExecuteScalarAsync<int>(countSql);
        var result = await connection.QueryAsync<User>(sql, new { Offset = (pageIndex - 1) * pageSize, PageSize = pageSize });
        return (result.ToList(), totalCount);
    }

    public async Task<(IReadOnlyList<User> Items, int TotalCount)> GetByAirlineIdAsync(Guid airlineId, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        const string countSql = "SELECT COUNT(*) FROM users.users WHERE airline_id = @AirlineId AND is_deleted = FALSE";
        const string sql = @"
            SELECT u.*, a.name as AirlineName, a.logo_url as AirlineLogoUrl 
            FROM users.users u 
            LEFT JOIN flights.airlines a ON u.airline_id = a.id 
            WHERE u.airline_id = @AirlineId AND u.is_deleted = FALSE
            ORDER BY u.created_at DESC
            OFFSET @Offset LIMIT @PageSize";

        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, new { AirlineId = airlineId });
        var result = await connection.QueryAsync<User>(sql, new { AirlineId = airlineId, Offset = (pageIndex - 1) * pageSize, PageSize = pageSize });
        return (result.ToList(), totalCount);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(@"
            SELECT u.*, a.name as AirlineName, a.logo_url as AirlineLogoUrl 
            FROM users.users u 
            LEFT JOIN flights.airlines a ON u.airline_id = a.id 
            WHERE u.id = @Id", 
            new { Id = id });
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        // Keeping EF for this one due to complex includes (RoleEntity, RolePermissions, Permission)
        // Dapper multi-mapping for 4 levels deep is complex and error-prone.
        return await _context.Users
            .Include(u => u.RoleEntity)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.RoleEntity)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(x => x.RefreshToken == refreshToken, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default)
    {
        return !await _context.Users.AnyAsync(x => x.Email == email, cancellationToken);
    }
}
