using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Notifications.Domain.Entities;

namespace AirlineTicket.Modules.Notifications.Application.Contracts;

public interface INotificationRepository
{
    Task AddAsync(Notification notification);
    Task<List<Notification>> GetPendingAsync(int batchSize = 10);
    Task<List<Notification>> GetAllAsync();
    Task MarkSentAsync(Guid id, DateTime? sentAt = null);
    Task MarkFailedAsync(Guid id, string errorMessage);
}

public interface INotificationSender
{
    // Returns (Sent, ErrorMessage)
    Task<(bool Success, string? Error)> SendAsync(Notification notification);
}

public interface ITemplateRepository
{
    Task<List<NotificationTemplate>> GetAllAsync();
    Task<Guid> CreateAsync(NotificationTemplate template);
}

public interface IEmailSender
{
    Task<(bool Success, string? Error)> SendAsync(string to, string subject, string body);
}

public interface ISmsSender
{
    Task<(bool Success, string? Error)> SendAsync(string phone, string message);
}
