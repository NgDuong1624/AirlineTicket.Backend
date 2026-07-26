using System.Collections.Generic;
using System.Threading.Tasks;
using AirlineTicket.Modules.Notifications.Application.Contracts;
using AirlineTicket.Modules.Notifications.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using AirlineTicket.Modules.Notifications.Infrastructure.Data;

namespace AirlineTicket.Modules.Notifications.Infrastructure.Services;

public class NotificationTemplateService : INotificationTemplateService
{
    private readonly NotificationDbContext _context;

    public NotificationTemplateService(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationTemplate?> GetTemplateAsync(string code, string language)
    {
        return await _context.NotificationTemplates
            .FirstOrDefaultAsync(t => t.Code == code && t.Language == language);
    }

    public string RenderTemplate(string template, Dictionary<string, string> parameters)
    {
        foreach (var param in parameters)
        {
            template = template.Replace($"{{{{{param.Key}}}}}", param.Value);
        }
        return template;
    }
}