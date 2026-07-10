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

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        var result = await connection.QueryAsync<User>(@"
            SELECT u.*, a.Name as AirlineName 
            FROM dbo.Users u 
            LEFT JOIN dbo.Airlines a ON u.AirlineId = a.Id");
        return result.ToList();
    }

    public async Task<IReadOnlyList<User>> GetByAirlineIdAsync(Guid airlineId, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        var result = await connection.QueryAsync<User>(@"
            SELECT u.*, a.Name as AirlineName 
            FROM dbo.Users u 
            LEFT JOIN dbo.Airlines a ON u.AirlineId = a.Id 
            WHERE u.AirlineId = @AirlineId AND u.IsDeleted = 0", 
            new { AirlineId = airlineId });
        return result.ToList();
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(@"
            SELECT u.*, a.Name as AirlineName 
            FROM dbo.Users u 
            LEFT JOIN dbo.Airlines a ON u.AirlineId = a.Id 
            WHERE u.Id = @Id", 
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
        var connection = _context.Database.GetDbConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM dbo.Users WHERE RefreshToken = @RefreshToken", 
            new { RefreshToken = refreshToken });
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
