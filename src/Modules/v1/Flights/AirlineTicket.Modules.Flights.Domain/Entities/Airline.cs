namespace AirlineTicket.Modules.Flights.Domain.Entities;

public class Airline
{
    public Guid Id { get; set; }
    public string IataCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
