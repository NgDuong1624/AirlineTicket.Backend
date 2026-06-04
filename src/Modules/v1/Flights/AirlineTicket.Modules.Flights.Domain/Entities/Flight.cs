using AirlineTicket.Modules.Flights.Domain.Enums;

namespace AirlineTicket.Modules.Flights.Domain.Entities;

public class Flight
{
    public Guid Id { get; set; }
    public Guid RouteId { get; set; }
    public Guid AirplaneId { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public DateTime ScheduledDeparture { get; set; }
    public DateTime ScheduledArrival { get; set; }
    public DateTime? ActualDeparture { get; set; }
    public DateTime? ActualArrival { get; set; }
    public FlightStatus Status { get; set; }

    public Route Route { get; set; } = null!;
    public Airplane Airplane { get; set; } = null!;
    public ICollection<FlightSeat> FlightSeats { get; set; } = new List<FlightSeat>();
}
