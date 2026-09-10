namespace AirlineTicket.Modules.Flights.Domain.Enums;

public enum FlightStatus
{
    Scheduled = 0,
    Delayed = 1,
    Boarding = 2,
    InAir = 3,
    Landed = 4,
    Cancelled = 5,
    CheckInOpen = 6,
    Departed = 7,
    EnRoute = 8,
    Approaching = 9,
    ArrivedAtGate = 10
}
