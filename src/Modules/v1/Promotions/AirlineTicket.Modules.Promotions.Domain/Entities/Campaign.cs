using System;

namespace AirlineTicket.Modules.Promotions.Domain.Entities;

public class Campaign
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? BannerUrl { get; set; }
    public string? Content { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsDeleted { get; set; }

    /// <summary>Null = global (admin) campaign; set = airline-scoped (partner) campaign.</summary>
    public Guid? AirlineId { get; set; }
}
