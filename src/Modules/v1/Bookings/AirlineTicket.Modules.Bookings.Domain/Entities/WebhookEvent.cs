using AirlineTicket.Modules.Bookings.Domain.Enums;

namespace AirlineTicket.Modules.Bookings.Domain.Entities;

public class WebhookEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public PaymentProvider Provider { get; set; }
    public string ProviderEventId { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public WebhookEventStatus Status { get; set; } = WebhookEventStatus.Received;
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? Error { get; set; }
}
