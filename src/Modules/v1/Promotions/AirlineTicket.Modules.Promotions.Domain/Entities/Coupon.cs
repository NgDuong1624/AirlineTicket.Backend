using AirlineTicket.Modules.Promotions.Domain.Enums;

namespace AirlineTicket.Modules.Promotions.Domain.Entities;

public class Coupon
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal MaxDiscountAmount { get; set; }
    
    public int MaxUsages { get; set; }
    public int CurrentUsages { get; set; }
    
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public bool IsActive { get; set; }
}
