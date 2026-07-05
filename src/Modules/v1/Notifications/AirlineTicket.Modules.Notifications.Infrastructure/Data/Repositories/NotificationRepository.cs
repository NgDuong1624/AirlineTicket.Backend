using AirlineTicket.Modules.Notifications.Application.Contracts;
using AirlineTicket.Modules.Notifications.Domain.Entities;
using AirlineTicket.Modules.Notifications.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Notifications.Infrastructure.Data.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _context;

    public NotificationRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Notification notification)
    {
        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Notification>> GetPendingAsync(int batchSize = 10)
    {
        return await _context.Notifications
            .Where(n => n.Status == 0) // Pending
            .OrderBy(n => n.CreatedAt)
            .Take(batchSize)
            .ToListAsync();
    }

    public async Task<List<Notification>> GetAllAsync()
    {
        return await _context.Notifications
            .OrderByDescending(n => n.CreatedAt)
            .Take(100)
            .ToListAsync();
    }

    public async Task MarkSentAsync(Guid id, DateTime? sentAt = null)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification != null)
        {
            notification.Status = 1; // Sent
            notification.SentAt = sentAt ?? DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkFailedAsync(Guid id, string errorMessage)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification != null)
        {
            notification.Status = 2; // Failed
            notification.ErrorMessage = errorMessage;
            notification.RetryCount++;
            await _context.SaveChangesAsync();
        }
    }
}

public class TemplateRepository : ITemplateRepository
{
    private readonly NotificationDbContext _context;

    public TemplateRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task<List<NotificationTemplate>> GetAllAsync()
    {
        return await _context.NotificationTemplates.ToListAsync();
    }

    public async Task<Guid> CreateAsync(NotificationTemplate template)
    {
        _context.NotificationTemplates.Add(template);
        await _context.SaveChangesAsync();
        return template.Id;
    }
}

