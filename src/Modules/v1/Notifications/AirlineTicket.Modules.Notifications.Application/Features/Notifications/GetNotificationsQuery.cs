using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Notifications.Domain.Entities;
using AirlineTicket.Modules.Notifications.Application.Contracts;

namespace AirlineTicket.Modules.Notifications.Application.Features;

public record GetNotificationsQuery() : IQuery<Result<List<Notification>>>;

public class GetNotificationsQueryHandler : IQueryHandler<GetNotificationsQuery, Result<List<Notification>>>
{
    private readonly INotificationRepository _repository;

    public GetNotificationsQueryHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<Notification>>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notifications = await _repository.GetAllAsync();
        return Result.Success(notifications);
    }
}