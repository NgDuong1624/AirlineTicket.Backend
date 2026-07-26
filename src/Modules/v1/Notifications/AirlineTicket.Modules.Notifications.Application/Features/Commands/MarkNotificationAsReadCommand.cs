using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Notifications.Application.Contracts;

namespace AirlineTicket.Modules.Notifications.Application.Features.Commands;

public record MarkNotificationAsReadCommand(Guid NotificationId, Guid UserId) : IRequest<bool>;

public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, bool>
{
    private readonly INotificationRepository _repository;

    public MarkNotificationAsReadCommandHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _repository.GetByIdAsync(request.NotificationId);

        if (notification == null || notification.UserId != request.UserId)
        {
            return false;
        }

        notification.IsRead = true;
        await _repository.SaveChangesAsync();
        return true;
    }
}