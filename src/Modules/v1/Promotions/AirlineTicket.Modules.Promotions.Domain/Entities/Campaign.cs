using AirlineTicket.Modules.Promotions.Domain.Enums;

namespace AirlineTicket.Modules.Promotions.Domain.Entities;

public class Campaign
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public bool IsActive { get; set; }

    // Điều kiện áp dụng
    public Guid? TargetAirlineId { get; set; }
    public Guid? TargetFlightId { get; set; }
}
