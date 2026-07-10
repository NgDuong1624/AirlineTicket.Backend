using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Bookings.Application.Contracts;

public class BookingDto
{
    public Guid Id { get; set; }
    public Guid FlightId { get; set; }
    public Guid? UserId { get; set; }
    public string PnrCode { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = "Pending";
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
}

public interface IBookingRepository
{
    Task<BookingDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BookingDetailDto?> GetDetailByPnrAsync(string pnrCode, CancellationToken cancellationToken = default);
    Task<List<BookingDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<(List<BookingDto> Items, int TotalCount)> GetAllAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(BookingDto booking, CancellationToken cancellationToken = default);
    Task UpdateAsync(BookingDto booking, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Persists a full booking (booking + passengers + tickets) in one transaction.</summary>
    Task CreateFullBookingAsync(NewBooking booking, CancellationToken cancellationToken = default);
}

/// <summary>Input model for persisting a complete staff booking graph.</summary>
public class NewBooking
{
    public Guid Id { get; set; }
    public Guid FlightId { get; set; }
    public Guid? UserId { get; set; }
    public string PnrCode { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = "Pending";
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public List<NewTicket> Tickets { get; set; } = new();
}

public class NewTicket
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string IdentityCard { get; set; } = string.Empty;
    public string SeatNumber { get; set; } = string.Empty;
    public Guid SeatId { get; set; }
}

public class TicketDto
{
    public Guid TicketId { get; set; }
    public Guid BookingId { get; set; }
    public string PassengerName { get; set; } = string.Empty;
    public string SeatNumber { get; set; } = string.Empty;
}

public class BookingDetailDto : BookingDto
{
    public List<TicketDto> Tickets { get; set; } = new();
}

public interface ITicketRepository
{
    Task<TicketDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(TicketDto ticket, CancellationToken cancellationToken = default);
}
