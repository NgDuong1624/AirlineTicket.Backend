using System;
using System.Collections.Generic;

namespace AirlineTicket.Modules.Flights.Domain.Entities;

public class Airline
{
    public Guid Id { get; set; }
    public string IataCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? BaseCountry { get; set; }
    public string? ApiEndpoint { get; set; }
    public string? ApiKey { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Airplane> Airplanes { get; set; } = new List<Airplane>();
    public virtual ICollection<Route> Routes { get; set; } = new List<Route>();
}
