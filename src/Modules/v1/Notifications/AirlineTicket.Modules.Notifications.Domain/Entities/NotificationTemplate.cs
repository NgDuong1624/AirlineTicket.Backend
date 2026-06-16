using System;

namespace AirlineTicket.Modules.Notifications.Domain.Entities;

public class NotificationTemplate
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty; // e.g., 'BOOKING_CONFIRMED', 'FLIGHT_DELAYED'
    public string Subject { get; set; } = string.Empty;
    public string BodyTemplate { get; set; } = string.Empty;
    public string? Language { get; set; } = "vi";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
