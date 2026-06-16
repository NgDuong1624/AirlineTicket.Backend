using System;

namespace AirlineTicket.Modules.Notifications.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string Recipient { get; set; } = string.Empty; // Email or Phone
    public string? Subject { get; set; }
    public string Content { get; set; } = string.Empty;
    public int Type { get; set; } // 0: Email, 1: SMS, 2: Push, 3: SignalR
    public int Status { get; set; } // 0: Pending, 1: Sent, 2: Failed
    public int RetryCount { get; set; } = 0;
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
