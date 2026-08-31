using System;

namespace AirlineTicket.Modules.Notifications.Application.DTOs;

public record NotificationDto(
    Guid Id,
    Guid? UserId,
    string Type,
    int Severity,
    string Title,
    string? Content,
    string? TemplateCode = null,
    string? TemplateParameters = null,
    string? ActionUrl = null,
    Guid? ReferenceId = null,
    string? ReferenceType = null,
    bool IsRead = false,
    bool IsDeleted = false,
    DateTime CreatedAt = default);
