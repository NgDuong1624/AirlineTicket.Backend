using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Notifications.Domain.Entities;

namespace AirlineTicket.Modules.Notifications.Application.Contracts;

public interface INotificationRepository
{
    Task AddAsync(Notification notification);
    Task<Notification?> GetByIdAsync(Guid id);
    Task<List<Notification>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize);
    Task<List<Notification>> GetAllAsync(int pageNumber, int pageSize);
    Task<int> GetUnreadCountByUserIdAsync(Guid userId);
    Task UpdateAsync(Notification notification);
    Task SaveChangesAsync();
}

public interface ITemplateRepository
{
    Task<NotificationTemplate?> GetByIdAsync(Guid id);
    Task<List<NotificationTemplate>> GetAllAsync();
    Task UpdateAsync(NotificationTemplate template);
    Task SaveChangesAsync();
}
