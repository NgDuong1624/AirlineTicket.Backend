using AirlineTicket.Modules.Flights.Domain.Enums;

namespace AirlineTicket.Modules.Flights.Domain.Entities;

public class FlightSeat
{
    public Guid Id { get; set; }
    public Guid FlightId { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public SeatClass SeatClass { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; } = true;

    public Flight Flight { get; set; } = null!;
}
