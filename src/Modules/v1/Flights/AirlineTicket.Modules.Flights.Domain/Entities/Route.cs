namespace AirlineTicket.Modules.Flights.Domain.Entities;

public class Route
{
    public Guid Id { get; set; }
    public Guid AirlineId { get; set; }
    public Guid OriginAirportId { get; set; }
    public Guid DestinationAirportId { get; set; }

    public Airline Airline { get; set; } = null!;
    public Airport OriginAirport { get; set; } = null!;
    public Airport DestinationAirport { get; set; } = null!;
}
