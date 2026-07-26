using AirlineTicket.Modules.Notifications.Application.Contracts;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Notifications.Application.Features.Commands;

public record MarkAllNotificationsAsReadCommand(Guid UserId) : IRequest<bool>;

public class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, bool>
{
    private readonly INotificationRepository _repository;

    public MarkAllNotificationsAsReadCommandHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
    {
        // Note: For performance, this should ideally be a bulk update in the repository.
        // For now, we fetch and update.
        var unreadNotifications = await _repository.GetByUserIdAsync(request.UserId, 1, 1000); // Fetch up to 1000 unread
        
        foreach (var notification in unreadNotifications)
        {
            if (!notification.IsRead)
            {
                notification.IsRead = true;
            }
        }

        await _repository.SaveChangesAsync();
        return true;
    }
}