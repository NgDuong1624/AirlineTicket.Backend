using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Domain.Entities;
using AirlineTicket.Modules.Bookings.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AirlineTicket.Modules.Bookings.Infrastructure.Data.Repositories;

public class GroupBookingRepository : IGroupBookingRepository
{
    private readonly BookingDbContext _context;

    public GroupBookingRepository(BookingDbContext context)
    {
        _context = context;
    }

    public async Task<GroupBooking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.GroupBookings
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<GroupBooking?> GetByInviteCodeAsync(string inviteCode, CancellationToken cancellationToken = default)
    {
        return await _context.GroupBookings
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.InviteCode == inviteCode, cancellationToken);
    }

    public async Task<List<GroupBooking>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.GroupBookings
            .Include(g => g.Members)
            .Where(g => g.LeaderUserId == userId || g.Members.Any(m => m.UserId == userId))
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<GroupBooking>> GetExpiredActiveGroupsAsync(DateTime cutoffUtc, CancellationToken cancellationToken = default)
    {
        return await _context.GroupBookings
            .Include(g => g.Members)
            .Where(g => g.Status == GroupBookingStatus.Active && g.ExpiresAt <= cutoffUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<GroupMember?> GetMemberByIdAsync(Guid memberId, CancellationToken cancellationToken = default)
    {
        return await _context.GroupMembers
            .Include(m => m.GroupBooking)
            .FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);
    }

    public async Task AddAsync(GroupBooking groupBooking, CancellationToken cancellationToken = default)
    {
        await _context.GroupBookings.AddAsync(groupBooking, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(GroupBooking groupBooking, CancellationToken cancellationToken = default)
    {
        _context.GroupBookings.Update(groupBooking);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
