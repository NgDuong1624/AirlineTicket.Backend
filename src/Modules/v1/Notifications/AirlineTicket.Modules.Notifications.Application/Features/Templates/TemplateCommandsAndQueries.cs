using AirlineTicket.Modules.Notifications.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;

namespace AirlineTicket.Modules.Notifications.Application.Features.Templates;

public record GetTemplatesQuery() : IRequest<List<NotificationTemplate>>;

public record GetTemplateByIdQuery(Guid Id) : IRequest<NotificationTemplate?>;

public record UpdateTemplateCommand(
    Guid Id,
    string Subject,
    string BodyTemplate,
    string Language) : IRequest<bool>;
