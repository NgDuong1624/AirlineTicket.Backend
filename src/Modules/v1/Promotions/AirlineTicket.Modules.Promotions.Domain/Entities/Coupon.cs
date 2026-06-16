using System;
using AirlineTicket.Modules.Promotions.Domain.Enums;

namespace AirlineTicket.Modules.Promotions.Domain.Entities;

public class Coupon
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal? MinOrderValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public int? UsageLimit { get; set; }
    public int UsageCount { get; set; } = 0;

    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}
