using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Bookings.Application.Contracts;

/// <summary>
/// Reads ticket-sales rows for the staff board. Implemented in the API host because the
/// data spans the Bookings and Flights DbContexts (flight number + route live in Flights).
/// </summary>
public interface IStaffSalesReader
{
    Task<(List<StaffSaleDto> Items, int TotalCount)> GetSalesAsync(
        int pageIndex,
        int pageSize,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default);
}

public class StaffSaleDto
{
    public string Id { get; set; } = string.Empty;            // PNR code
    public Guid BookingId { get; set; }
    public string PassengerName { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;          // "SGN → HAN"
    public DateTime? DepartureAt { get; set; }
    public string SeatClass { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime BookedAt { get; set; }
}
