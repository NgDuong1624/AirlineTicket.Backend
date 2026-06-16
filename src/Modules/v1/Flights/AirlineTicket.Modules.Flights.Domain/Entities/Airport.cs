using System;
using System.Collections.Generic;

namespace AirlineTicket.Modules.Flights.Domain.Entities;

public class Airport
{
    public Guid Id { get; set; }
    public string IataCode { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameVi { get; set; } = string.Empty;
    public string CityEn { get; set; } = string.Empty;
    public string CityVi { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }

    // Navigation properties
    public virtual ICollection<Route> OriginRoutes { get; set; } = new List<Route>();
    public virtual ICollection<Route> DestinationRoutes { get; set; } = new List<Route>();
}
