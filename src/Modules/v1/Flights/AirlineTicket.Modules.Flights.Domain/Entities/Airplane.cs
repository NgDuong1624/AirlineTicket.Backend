using System;
using System.Collections.Generic;

namespace AirlineTicket.Modules.Flights.Domain.Entities;

public class Airplane
{
    public Guid Id { get; set; }
    public Guid AirlineId { get; set; }
    public Guid? AircraftModelId { get; set; }
    public string Model { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public int TotalCapacity { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation properties
    public virtual Airline Airline { get; set; } = null!;
    public virtual AircraftModel? AircraftModel { get; set; }
    public virtual ICollection<AirplaneSeat> AirplaneSeats { get; set; } = new List<AirplaneSeat>();
    public virtual ICollection<Flight> Flights { get; set; } = new List<Flight>();
}
