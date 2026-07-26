using AirlineTicket.Modules.Notifications.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Notifications.Application.Contracts;

public interface INotificationTemplateService
{
    Task<NotificationTemplate?> GetTemplateAsync(string code, string language);
    string RenderTemplate(string template, Dictionary<string, string> parameters);
}