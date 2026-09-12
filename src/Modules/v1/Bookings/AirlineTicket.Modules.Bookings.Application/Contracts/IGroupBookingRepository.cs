using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Bookings.Domain.Entities;

namespace AirlineTicket.Modules.Bookings.Application.Contracts;

public interface IGroupBookingRepository
{
    Task<GroupBooking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GroupBooking?> GetByInviteCodeAsync(string inviteCode, CancellationToken cancellationToken = default);
    Task<List<GroupBooking>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<GroupBooking>> GetExpiredActiveGroupsAsync(DateTime cutoffUtc, CancellationToken cancellationToken = default);
    Task<GroupMember?> GetMemberByIdAsync(Guid memberId, CancellationToken cancellationToken = default);
    Task AddAsync(GroupBooking groupBooking, CancellationToken cancellationToken = default);
    Task UpdateAsync(GroupBooking groupBooking, CancellationToken cancellationToken = default);
}
