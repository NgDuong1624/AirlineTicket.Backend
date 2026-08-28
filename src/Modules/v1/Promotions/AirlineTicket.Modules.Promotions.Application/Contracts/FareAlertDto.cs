using System;

namespace AirlineTicket.Modules.Promotions.Application.Contracts;

public class FareAlertDto
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
    public bool IsActive { get; set; }
    public DateTime LastCheckedAt { get; set; }
    public DateTime? LastNotifiedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateFareAlertRequest
{
    public Guid OriginAirportId { get; set; }
    public Guid DestinationAirportId { get; set; }
    public DateOnly DepartureDate { get; set; }
    public DateOnly? ReturnDate { get; set; }
    public decimal TargetPrice { get; set; }
    public decimal CurrentLowestPrice { get; set; }
    public string Currency { get; set; } = "VND";
}

public class UpdateFareAlertRequest
{
    public decimal? TargetPrice { get; set; }
    public bool? IsActive { get; set; }
}
