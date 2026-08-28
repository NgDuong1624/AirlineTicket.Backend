using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using AirlineTicket.Modules.Promotions.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AirlineTicket.Modules.Promotions.Infrastructure.Data.Repositories;

public class FareAlertRepository : IFareAlertRepository
{
    private readonly PromotionDbContext _context;

    public FareAlertRepository(PromotionDbContext context)
    {
        _context = context;
    }

    public async Task<FareAlert?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.FareAlerts
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<FareAlert>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.FareAlerts
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<FareAlert?> GetUserAlertForRouteAsync(
        Guid userId,
        Guid originAirportId,
        Guid destinationAirportId,
        DateOnly departureDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.FareAlerts
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.OriginAirportId == originAirportId &&
                x.DestinationAirportId == destinationAirportId &&
                x.DepartureDate == departureDate,
                cancellationToken);
    }

    public async Task<int> CountActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.FareAlerts
            .CountAsync(x => x.UserId == userId && x.IsActive, cancellationToken);
    }

    public async Task<List<FareAlert>> GetActiveAlertsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.FareAlerts
            .Where(x => x.IsActive && x.DepartureDate >= DateOnly.FromDateTime(DateTime.UtcNow))
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid> CreateAsync(FareAlert alert, CancellationToken cancellationToken = default)
    {
        _context.FareAlerts.Add(alert);
        await _context.SaveChangesAsync(cancellationToken);
        return alert.Id;
    }

    public async Task<bool> UpdateAsync(FareAlert alert, CancellationToken cancellationToken = default)
    {
        alert.UpdatedAt = DateTime.UtcNow;
        _context.FareAlerts.Update(alert);
        return await _context.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var alert = await _context.FareAlerts
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);

        if (alert == null)
            return false;

        _context.FareAlerts.Remove(alert);
        return await _context.SaveChangesAsync(cancellationToken) > 0;
    }
}
