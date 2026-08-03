namespace AirlineTicket.Modules.Notifications.Application.DTOs;

public record NotificationDto(Guid Id, Guid UserId, string Title, string Message, DateTime CreatedAt);
