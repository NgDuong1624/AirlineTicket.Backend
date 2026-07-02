using System;

namespace AirlineTicket.Modules.Interactions.Domain.Entities;

public class ChatMessage
{
    public Guid Id { get; set; }
    public Guid AirlineId { get; set; }
    public string SenderRole { get; set; } = string.Empty; // "Customer" or "Staff"
    public string SenderName { get; set; } = string.Empty;
    public string? CustomerConnectionId { get; set; }
    public string? StaffConnectionId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}