using AirlineTicket.Modules.Flights.Domain.Enums;

namespace AirlineTicket.Modules.Flights.Domain.Entities;

public class AirplaneSeat
{
    public Guid Id { get; set; }
    public Guid AirplaneId { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public SeatClass SeatClass { get; set; }
    public decimal PriceMultiplier { get; set; }

    public Airplane Airplane { get; set; } = null!;
}
