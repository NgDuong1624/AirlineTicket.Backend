using AirlineTicket.Modules.Notifications.Application.Contracts;
using AirlineTicket.Modules.Notifications.Domain.Entities;
using AirlineTicket.Modules.Notifications.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Notifications.Infrastructure.Data.Repositories;

public class TemplateRepository : ITemplateRepository
{
    private readonly NotificationDbContext _context;

    public TemplateRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(NotificationTemplate template)
    {
        await _context.NotificationTemplates.AddAsync(template);
    }

    public async Task<NotificationTemplate?> GetByIdAsync(Guid id)
    {
        return await _context.NotificationTemplates.FindAsync(id);
    }

    public async Task<List<NotificationTemplate>> GetAllAsync()
    {
        return await _context.NotificationTemplates.ToListAsync();
    }

    public async Task UpdateAsync(NotificationTemplate template)
    {
        _context.NotificationTemplates.Update(template);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(NotificationTemplate template)
    {
        _context.NotificationTemplates.Remove(template);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}