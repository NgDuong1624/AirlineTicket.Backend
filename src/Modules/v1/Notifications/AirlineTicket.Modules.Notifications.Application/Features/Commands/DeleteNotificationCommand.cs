using AirlineTicket.Modules.Notifications.Application.Contracts;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Notifications.Application.Features.Commands;

public record DeleteNotificationCommand(Guid NotificationId, Guid UserId) : IRequest<bool>;

public class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand, bool>
{
    private readonly INotificationRepository _repository;

    public DeleteNotificationCommandHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = await _repository.GetByIdAsync(request.NotificationId);

        if (notification == null || notification.UserId != request.UserId)
        {
            return false;
        }

        notification.IsDeleted = true;
        await _repository.SaveChangesAsync();
        return true;
    }
}