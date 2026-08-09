using AirlineTicket.Modules.Notifications.Application.Contracts;
using AirlineTicket.Modules.Notifications.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Notifications.Application.Features.Templates;

public class TemplateHandlers : 
    IRequestHandler<GetTemplatesQuery, List<NotificationTemplate>>,
    IRequestHandler<GetTemplateByIdQuery, NotificationTemplate?>,
    IRequestHandler<UpdateTemplateCommand, bool>
{
    private readonly ITemplateRepository _repository;

    public TemplateHandlers(ITemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<NotificationTemplate>> Handle(GetTemplatesQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetAllAsync();
    }

    public async Task<NotificationTemplate?> Handle(GetTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(request.Id);
    }

    public async Task<bool> Handle(UpdateTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _repository.GetByIdAsync(request.Id);
        if (template == null) return false;

        template.Subject = request.Subject;
        template.BodyTemplate = request.BodyTemplate;
        template.Language = request.Language;

        await _repository.UpdateAsync(template);
        await _repository.SaveChangesAsync();
        return true;
    }
}