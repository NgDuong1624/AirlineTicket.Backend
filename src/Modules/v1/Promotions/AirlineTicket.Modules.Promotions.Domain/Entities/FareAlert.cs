using System;

namespace AirlineTicket.Modules.Promotions.Domain.Entities;

public class FareAlert
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid OriginAirportId { get; set; }
    public Guid DestinationAirportId { get; set; }
    public DateOnly DepartureDate { get; set; }
    public DateOnly? ReturnDate { get; set; }
    public decimal TargetPrice { get; set; }
    public decimal CurrentLowestPrice { get; set; }
    public decimal? LastNotifiedPrice { get; set; }
    public string Currency { get; set; } = "VND";
    public bool IsActive { get; set; } = true;
    public DateTime LastCheckedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastNotifiedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}