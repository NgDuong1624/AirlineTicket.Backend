namespace AirlineTicket.Modules.Flights.Domain.Entities;

public class Airport
{
    public Guid Id { get; set; }
    public string IataCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
}
