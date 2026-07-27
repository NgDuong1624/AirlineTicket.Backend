using System;

namespace AirlineTicket.Modules.Notifications.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; } // Recipient User ID
    public string Type { get; set; } = string.Empty; // Notification category: FlightCreated, FlightStatusChanged, ProfileUpdated, SystemError
    public int Severity { get; set; } // 0: Info, 1: Critical
    public string Title { get; set; } = string.Empty; // Notification title
    public string? Content { get; set; }
    public string? TemplateCode { get; set; } // Added for dynamic rendering
    public string? TemplateParameters { get; set; } // JSON string
    public string? ActionUrl { get; set; } // Deep link to redirect on click
    public Guid? ReferenceId { get; set; } // Source entity ID (FlightId, AirlineId, etc.)
    public string? ReferenceType { get; set; } // Source entity type: Flight, Airline, Booking
    public bool IsRead { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
