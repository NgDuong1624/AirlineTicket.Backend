using System;

namespace AirlineTicket.Modules.Promotions.Domain.Entities;

public class Campaign
{
    public Guid Id { get; set; }
    public string TitleEn { get; set; } = string.Empty;
    public string TitleVi { get; set; } = string.Empty;
    public string? BannerUrl { get; set; }
    public string? ContentEn { get; set; }
    public string? ContentVi { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsDeleted { get; set; }

    /// <summary>Null = global (admin) campaign; set = airline-scoped (partner) campaign.</summary>
    public Guid? AirlineId { get; set; }
}
