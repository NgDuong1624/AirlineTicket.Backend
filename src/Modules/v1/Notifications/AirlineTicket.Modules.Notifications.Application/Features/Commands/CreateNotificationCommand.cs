using AirlineTicket.Modules.Notifications.Application.Contracts;
using AirlineTicket.Modules.Notifications.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Notifications.Application.Features.Commands;

public record CreateNotificationCommand(
    Guid? UserId,
    string TemplateCode,
    Dictionary<string, string> TemplateParameters,
    int Severity,
    string? ActionUrl,
    Guid? ReferenceId,
    string? ReferenceType,
    string? Language = "en") : IRequest<Guid>;

public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, Guid>
{
    private readonly INotificationRepository _repository;
    private readonly INotificationTemplateService _templateService;

    public CreateNotificationCommandHandler(INotificationRepository repository, INotificationTemplateService templateService)
    {
        _repository = repository;
        _templateService = templateService;
    }

    public async Task<Guid> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        var template = await _templateService.GetTemplateAsync(request.TemplateCode, request.Language ?? "en");

        if (template == null)
        {
            // Fallback or error handling if template not found
            // For now, let's throw an exception or log an error
            throw new InvalidOperationException($"Notification template '{request.TemplateCode}' for language '{request.Language}' not found.");
        }

        var title = _templateService.RenderTemplate(template.Subject, request.TemplateParameters);
        var content = _templateService.RenderTemplate(template.BodyTemplate, request.TemplateParameters);

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Type = request.TemplateCode, // Use template code as notification type
            Severity = request.Severity,
            Title = title,
            Content = content,
            ActionUrl = request.ActionUrl,
            ReferenceId = request.ReferenceId,
            ReferenceType = request.ReferenceType,
            IsRead = false,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(notification);
        await _repository.SaveChangesAsync();

        // TODO: Trigger SignalR push here

        return notification.Id;
    }
}