using System;

namespace AirlineTicket.SignalR.Hubs;

public class FareAlertNotificationDto
{
    public Guid AlertId { get; set; }
    public Guid UserId { get; set; }
    public Guid OriginAirportId { get; set; }
    public Guid DestinationAirportId { get; set; }
    public DateOnly DepartureDate { get; set; }
    public decimal PreviousPrice { get; set; }
    public decimal NewPrice { get; set; }
    public decimal TargetPrice { get; set; }
    public string Currency { get; set; } = "VND";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class RoutePriceUpdateDto
{
    public Guid OriginAirportId { get; set; }
    public Guid DestinationAirportId { get; set; }
    public DateOnly DepartureDate { get; set; }
    public decimal LowestPrice { get; set; }
    public string Currency { get; set; } = "VND";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public interface IFareAlertClient
{
    Task PriceDropped(FareAlertNotificationDto payload);
    Task RoutePriceUpdated(RoutePriceUpdateDto payload);
}
