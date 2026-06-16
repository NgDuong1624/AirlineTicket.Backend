using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace AirlineTicket.Modules.Bookings.Infrastructure.Data.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly BookingDbContext _context;

    public BookingRepository(BookingDbContext context)
    {
        _context = context;
    }

    public async Task<BookingDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // TODO: Implement when entities are properly set up
        return await Task.FromResult<BookingDto?>(null);
    }

    public async Task<List<BookingDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // TODO: Implement when entities are properly set up
        return await Task.FromResult(new List<BookingDto>());
    }

    public async Task<List<BookingDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Implement when entities are properly set up
        return await Task.FromResult(new List<BookingDto>());
    }

    public async Task<Guid> CreateAsync(BookingDto booking, CancellationToken cancellationToken = default)
    {
        // TODO: Implement when entities are properly set up
        return await Task.FromResult(Guid.NewGuid());
    }

    public async Task UpdateAsync(BookingDto booking, CancellationToken cancellationToken = default)
    {
        // TODO: Implement when entities are properly set up
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // TODO: Implement when entities are properly set up
        await Task.CompletedTask;
    }
}

public class TicketRepository : ITicketRepository
{
    private readonly BookingDbContext _context;

    public TicketRepository(BookingDbContext context)
    {
        _context = context;
    }

    public async Task<TicketDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // TODO: Implement when entities are properly set up
        return await Task.FromResult<TicketDto?>(null);
    }

    public async Task<Guid> CreateAsync(TicketDto ticket, CancellationToken cancellationToken = default)
    {
        // TODO: Implement when entities are properly set up
        return await Task.FromResult(Guid.NewGuid());
    }
}