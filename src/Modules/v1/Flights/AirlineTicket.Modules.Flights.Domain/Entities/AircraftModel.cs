using System;
using System.Collections.Generic;

namespace AirlineTicket.Modules.Flights.Domain.Entities;

public class AircraftModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation properties
    public virtual ICollection<Airplane> Airplanes { get; set; } = new List<Airplane>();
    public virtual ICollection<AircraftModelSeatTemplate> SeatTemplates { get; set; } = new List<AircraftModelSeatTemplate>();
}
