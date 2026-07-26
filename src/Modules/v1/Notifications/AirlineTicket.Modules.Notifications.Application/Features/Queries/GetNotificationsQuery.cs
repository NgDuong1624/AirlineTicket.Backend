using AirlineTicket.Modules.Notifications.Application.Contracts;
using AirlineTicket.Modules.Notifications.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Notifications.Application.Features.Queries;

public record GetNotificationsQuery(Guid UserId, int PageNumber, int PageSize) : IRequest<List<Notification>>;

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, List<Notification>>
{
    private readonly INotificationRepository _repository;

    public GetNotificationsQueryHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Notification>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByUserIdAsync(request.UserId, request.PageNumber, request.PageSize);
    }
}