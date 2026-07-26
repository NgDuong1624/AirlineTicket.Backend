using AirlineTicket.Modules.Notifications.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;

namespace AirlineTicket.Modules.Notifications.Application.Features.Templates;

public record GetTemplatesQuery() : IRequest<List<NotificationTemplate>>;

public record GetTemplateByIdQuery(Guid Id) : IRequest<NotificationTemplate?>;

public record CreateTemplateCommand(
    string Code,
    string Subject,
    string BodyTemplate,
    string Language) : IRequest<Guid>;

public record UpdateTemplateCommand(
    Guid Id,
    string Code,
    string Subject,
    string BodyTemplate,
    string Language) : IRequest<bool>;

public record DeleteTemplateCommand(Guid Id) : IRequest<bool>;
