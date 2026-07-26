using AirlineTicket.Modules.Notifications.Application.Contracts;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Notifications.Application.Features.Queries;

public record GetUnreadNotificationCountQuery(Guid UserId) : IRequest<int>;

public class GetUnreadNotificationCountQueryHandler : IRequestHandler<GetUnreadNotificationCountQuery, int>
{
    private readonly INotificationRepository _repository;

    public GetUnreadNotificationCountQueryHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> Handle(GetUnreadNotificationCountQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetUnreadCountByUserIdAsync(request.UserId);
    }
}