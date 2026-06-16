using System;

namespace AirlineTicket.Modules.Bookings.Domain.Entities;

public class Passenger
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int? Gender { get; set; } // 0: Male, 1: Female, 2: Other
    public DateTime DateOfBirth { get; set; }
    public string? Nationality { get; set; }
    public string PassportNumber { get; set; } = string.Empty;
    public DateTime PassportExpiryDate { get; set; }

    // Navigation properties
    public virtual Booking Booking { get; set; } = null!;
}
