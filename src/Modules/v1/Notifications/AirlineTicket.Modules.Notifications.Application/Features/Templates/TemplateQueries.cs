using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Notifications.Domain.Entities;
using AirlineTicket.Modules.Notifications.Application.Contracts;

namespace AirlineTicket.Modules.Notifications.Application.Features.Templates;

public record GetTemplatesQuery() : IQuery<Result<List<NotificationTemplate>>>;

public class GetTemplatesQueryHandler : IQueryHandler<GetTemplatesQuery, Result<List<NotificationTemplate>>>
{
    private readonly ITemplateRepository _repository;

    public GetTemplatesQueryHandler(ITemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<NotificationTemplate>>> Handle(GetTemplatesQuery request, CancellationToken cancellationToken)
    {
        var templates = await _repository.GetAllAsync();
        return Result.Success(templates);
    }
}

public record CreateTemplateCommand(string Code, string Subject, string BodyTemplate, string? Language) : ICommand<Result<Guid>>;

public class CreateTemplateCommandHandler : ICommandHandler<CreateTemplateCommand, Result<Guid>>
{
    private readonly ITemplateRepository _repository;

    public CreateTemplateCommandHandler(ITemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(CreateTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = new NotificationTemplate
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Subject = request.Subject,
            BodyTemplate = request.BodyTemplate,
            Language = request.Language ?? "vi"
        };
        var id = await _repository.CreateAsync(template);
        return Result.Success(id);
    }
}