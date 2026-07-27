using AirlineTicket.Modules.Notifications.Application.Contracts;
using AirlineTicket.Modules.Notifications.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Notifications.Application.Features.Queries;

public record GetNotificationsQuery(Guid UserId, int PageNumber, int PageSize, string Language = "en") : IRequest<List<Notification>>;

public record GetAllNotificationsQuery(int PageNumber, int PageSize, string Language = "en") : IRequest<List<Notification>>;

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, List<Notification>>
{
    private readonly INotificationRepository _repository;
    private readonly INotificationTemplateService _templateService;

    public GetNotificationsQueryHandler(INotificationRepository repository, INotificationTemplateService templateService)
    {
        _repository = repository;
        _templateService = templateService;
    }

    public async Task<List<Notification>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notifications = await _repository.GetByUserIdAsync(request.UserId, request.PageNumber, request.PageSize);
        return await RenderNotifications(notifications, request.Language);
    }

    private async Task<List<Notification>> RenderNotifications(List<Notification> notifications, string language)
    {
        foreach (var notification in notifications)
        {
            if (!string.IsNullOrEmpty(notification.TemplateCode) && !string.IsNullOrEmpty(notification.TemplateParameters))
            {
                try
                {
                    var template = await _templateService.GetTemplateAsync(notification.TemplateCode, language);
                    if (template != null)
                    {
                        var parameters = JsonSerializer.Deserialize<Dictionary<string, string>>(notification.TemplateParameters);
                        if (parameters != null)
                        {
                            notification.Title = _templateService.RenderTemplate(template.Subject, parameters);
                            notification.Content = _templateService.RenderTemplate(template.BodyTemplate, parameters);
                        }
                    }
                }
                catch
                {
                    // Fallback to stored static title/content if translation fails
                }
            }
        }
        return notifications;
    }
}

public class GetAllNotificationsQueryHandler : IRequestHandler<GetAllNotificationsQuery, List<Notification>>
{
    private readonly INotificationRepository _repository;
    private readonly INotificationTemplateService _templateService;

    public GetAllNotificationsQueryHandler(INotificationRepository repository, INotificationTemplateService templateService)
    {
        _repository = repository;
        _templateService = templateService;
    }

    public async Task<List<Notification>> Handle(GetAllNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notifications = await _repository.GetAllAsync(request.PageNumber, request.PageSize);
        return await RenderNotifications(notifications, request.Language);
    }

    private async Task<List<Notification>> RenderNotifications(List<Notification> notifications, string language)
    {
        foreach (var notification in notifications)
        {
            if (!string.IsNullOrEmpty(notification.TemplateCode) && !string.IsNullOrEmpty(notification.TemplateParameters))
            {
                try
                {
                    var template = await _templateService.GetTemplateAsync(notification.TemplateCode, language);
                    if (template != null)
                    {
                        var parameters = JsonSerializer.Deserialize<Dictionary<string, string>>(notification.TemplateParameters);
                        if (parameters != null)
                        {
                            notification.Title = _templateService.RenderTemplate(template.Subject, parameters);
                            notification.Content = _templateService.RenderTemplate(template.BodyTemplate, parameters);
                        }
                    }
                }
                catch
                {
                    // Fallback to stored static title/content if translation fails
                }
            }
        }
        return notifications;
    }
}