using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Notifications.Application.DTOs;
using AirlineTicket.Modules.Notifications.Application.Contracts;
using AirlineTicket.Modules.Notifications.Domain.Entities;
using MediatR;
using System.Collections.Generic;

namespace AirlineTicket.Modules.Notifications.Application.Features.Commands;

public record CreateNotificationCommand(
    Guid? UserId,
    string TemplateCode,
    Dictionary<string, string> TemplateParameters,
    int Severity,
    string? ActionUrl,
    Guid? ReferenceId,
    string? ReferenceType,
    string? Language = "en",
    Guid? TargetAirlineId = null) : IRequest<Guid>;

public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, Guid>
{
    private readonly INotificationRepository _repository;
    private readonly INotificationTemplateService _templateService;
    private readonly INotificationPusher _notificationPusher;

    public CreateNotificationCommandHandler(INotificationRepository repository, INotificationTemplateService templateService, INotificationPusher notificationPusher)
    {
        _repository = repository;
        _templateService = templateService;
        _notificationPusher = notificationPusher;
    }

    public async Task<Guid> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        var template = await _templateService.GetTemplateAsync(request.TemplateCode, request.Language ?? "en");

        if (template == null)
        {
            throw new InvalidOperationException($"Notification template '{request.TemplateCode}' for language '{request.Language}' not found.");
        }

        var title = _templateService.RenderTemplate(template.Subject, request.TemplateParameters);
        var content = _templateService.RenderTemplate(template.BodyTemplate, request.TemplateParameters);

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Type = request.TemplateCode,
            Severity = request.Severity,
            Title = title,
            Content = content,
            TemplateCode = request.TemplateCode,
            TemplateParameters = JsonSerializer.Serialize(request.TemplateParameters),
            ActionUrl = request.ActionUrl,
            ReferenceId = request.ReferenceId,
            ReferenceType = request.ReferenceType,
            IsRead = false,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(notification);
        await _repository.SaveChangesAsync();

        if (request.UserId.HasValue)
        {
            var notificationDto = new NotificationDto(
                notification.Id,
                request.UserId.Value,
                notification.Title,
                notification.Content,
                notification.CreatedAt);
            await _notificationPusher.PushNotificationAsync(request.UserId.Value, notificationDto, cancellationToken);
        }
        else if (request.TargetAirlineId.HasValue)
        {
            var notificationDto = new NotificationDto(
                notification.Id,
                null,
                notification.Title,
                notification.Content,
                notification.CreatedAt);
            await _notificationPusher.PushToAirlineStaffAsync(request.TargetAirlineId.Value, notificationDto, cancellationToken);
        }

        return notification.Id;
    }
}