using AirlineTicket.Modules.Flights.Domain.Entities;
using AirlineTicket.Modules.Users.Application.Repositories;
using AirlineTicket.Modules.Users.Domain.Entities;
using AirlineTicket.Modules.Users.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AirlineTicket.Modules.Users.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserDbContext _context;

    public UserRepository(UserDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<User> Items, int TotalCount)> GetAllAsync(string? search, Guid? airlineId, int? roleId, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsNoTracking();

        if (roleId.HasValue && roleId.Value != -1)
        {
            query = query.Where(u => u.Role == roleId.Value);
        }

        if (airlineId.HasValue)
        {
            query = query.Where(u => u.AirlineId == airlineId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();

            query = query.Where(u =>
                (u.Email != null && u.Email.Contains(keyword)) ||
                (u.FullName != null && u.FullName.Contains(keyword)) ||
                (u.Phone != null && u.Phone.Contains(keyword))
            );
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new User
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                Phone = u.Phone,
                IsActive = u.IsActive,
                IsDeleted = u.IsDeleted,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                AirlineId = u.AirlineId,
                Role = u.Role,
                AvatarUrl = u.AvatarUrl,
                LanguagePreference = u.LanguagePreference,
                AirlineName = u.AirlineId.HasValue ? _context.Airlines.Where(a => a.Id == u.AirlineId).Select(a => a.Name).FirstOrDefault() : null,
                AirlineLogoUrl = u.AirlineId.HasValue ? _context.Airlines.Where(a => a.Id == u.AirlineId).Select(a => a.LogoUrl).FirstOrDefault() : null
            })
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(IReadOnlyList<User> Items, int TotalCount)> GetByAirlineIdAsync(Guid airlineId, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Users
            .AsNoTracking()
            .Where(u => u.AirlineId == airlineId && !u.IsDeleted);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new User
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                Phone = u.Phone,
                IsActive = u.IsActive,
                IsDeleted = u.IsDeleted,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                AirlineId = u.AirlineId,
                Role = u.Role,
                AvatarUrl = u.AvatarUrl,
                LanguagePreference = u.LanguagePreference,
                AirlineName = u.AirlineId.HasValue ? _context.Airlines.Where(a => a.Id == u.AirlineId).Select(a => a.Name).FirstOrDefault() : null,
                AirlineLogoUrl = u.AirlineId.HasValue ? _context.Airlines.Where(a => a.Id == u.AirlineId).Select(a => a.LogoUrl).FirstOrDefault() : null
            })
            .ToListAsync(cancellationToken);
            
        return (items, totalCount);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
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
