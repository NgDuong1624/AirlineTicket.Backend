namespace AirlineTicket.Modules.Flights.Domain.Entities;

public class Airplane
{
    public Guid Id { get; set; }
    public Guid AirlineId { get; set; }
    public string Model { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public int TotalCapacity { get; set; }

    public Airline Airline { get; set; } = null!;
    public ICollection<AirplaneSeat> Seats { get; set; } = new List<AirplaneSeat>();
}
