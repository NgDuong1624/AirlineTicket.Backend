using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Domain.Entities;
using AirlineTicket.Modules.Bookings.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Dapper;

namespace AirlineTicket.Modules.Bookings.Infrastructure.Data.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly BookingDbContext _context;

    public BookingRepository(BookingDbContext context)
    {
        _context = context;
    }

    public async Task<BookingDetailDto?> GetDetailByPnrAsync(string pnrCode, CancellationToken cancellationToken = default)
    {
        var booking = await _context.Bookings
            .AsNoTracking()
            .Include(b => b.Tickets)
                .ThenInclude(t => t.Passenger)
            .FirstOrDefaultAsync(b => b.PnrCode == pnrCode, cancellationToken);

        if (booking == null) return null;

        return new BookingDetailDto
        {
            Id = booking.Id,
            PnrCode = booking.PnrCode,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status.ToString(),
            ContactEmail = booking.ContactEmail,
            ContactPhone = booking.ContactPhone,
            Tickets = booking.Tickets.Select(t => new TicketDto
            {
                TicketId = t.Id,
                BookingId = t.BookingId,
                PassengerName = t.Passenger != null ? $"{t.Passenger.FirstName} {t.Passenger.LastName}" : string.Empty,
                SeatNumber = t.TicketNumber
            }).ToList()
        };
    }

    public async Task<BookingDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        const string sql = @"
            SELECT Id, UserId, PnrCode, TotalPrice, Status, ContactEmail, ContactPhone, CreatedAt, UpdatedAt
            FROM dbo.Bookings 
            WHERE Id = @Id";
        
        return await connection.QueryFirstOrDefaultAsync<BookingDto>(sql, new { Id = id });
    }

    public async Task<List<BookingDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        const string sql = @"
            SELECT Id, UserId, PnrCode, TotalPrice, Status, ContactEmail, ContactPhone, CreatedAt, UpdatedAt
            FROM dbo.Bookings 
            WHERE UserId = @UserId
            ORDER BY CreatedAt DESC";
        
        var result = await connection.QueryAsync<BookingDto>(sql, new { UserId = userId });
        return result.ToList();
    }

    public async Task<(List<BookingDto> Items, int TotalCount)> GetAllAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        const string sql = @"
            SELECT Id, UserId, PnrCode, TotalPrice, Status, ContactEmail, ContactPhone, CreatedAt, UpdatedAt
            FROM dbo.Bookings 
            ORDER BY CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
        
        const string countSql = "SELECT COUNT(*) FROM dbo.Bookings";
        
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql);
        var result = await connection.QueryAsync<BookingDto>(sql, new { Offset = (pageIndex - 1) * pageSize, PageSize = pageSize });
        return (result.ToList(), totalCount);
    }

    public async Task<Guid> CreateAsync(BookingDto bookingDto, CancellationToken cancellationToken = default)
    {
        var booking = new Booking
        {
            Id = bookingDto.Id == Guid.Empty ? Guid.NewGuid() : bookingDto.Id,
            UserId = bookingDto.UserId ?? Guid.Empty,
            PnrCode = bookingDto.PnrCode,
            TotalPrice = bookingDto.TotalPrice,
            Status = ParseStatus(bookingDto.Status),
            ContactEmail = bookingDto.ContactEmail,
            ContactPhone = bookingDto.ContactPhone,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync(cancellationToken);
        return booking.Id;
    }

    public async Task UpdateAsync(BookingDto bookingDto, CancellationToken cancellationToken = default)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingDto.Id, cancellationToken);
        if (booking is null) return;

        booking.Status = ParseStatus(bookingDto.Status);
        booking.ContactEmail = bookingDto.ContactEmail;
        booking.ContactPhone = bookingDto.ContactPhone;
        booking.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (booking is null) return;

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateFullBookingAsync(NewBooking input, CancellationToken cancellationToken = default)
    {
        var booking = new Booking
        {
            Id = input.Id == Guid.Empty ? Guid.NewGuid() : input.Id,
            UserId = input.UserId ?? Guid.Empty,
            PnrCode = input.PnrCode,
            TotalPrice = input.TotalPrice,
            Currency = "USD",
            Status = ParseStatus(input.Status),
            ContactEmail = input.ContactEmail,
            ContactPhone = input.ContactPhone,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        foreach (var t in input.Tickets)
        {
            var passenger = new Passenger
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                FirstName = t.FirstName,
                LastName = t.LastName,
                PassportNumber = t.IdentityCard
            };
            booking.Passengers.Add(passenger);

            booking.Tickets.Add(new Ticket
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                PassengerId = passenger.Id,
                FlightId = input.FlightId,
                SeatId = t.SeatId,
                TicketNumber = $"TK-{booking.PnrCode}-{t.SeatNumber}",
                Status = TicketStatus.Valid
            });
        }

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<StaffSaleBookingDto>> GetStaffSalesBookingsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .AsNoTracking()
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new StaffSaleBookingDto
            {
                Id = b.Id,
                PnrCode = b.PnrCode,
                TotalPrice = b.TotalPrice,
                Status = b.Status.ToString(),
                CreatedAt = b.CreatedAt,
                PassengerName = b.Passengers.Select(p => p.FirstName + " " + p.LastName).FirstOrDefault() ?? string.Empty,
                FlightId = b.Tickets.Select(t => (Guid?)t.FlightId).FirstOrDefault(),
                SeatId = b.Tickets.Select(t => (Guid?)t.SeatId).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<BookingConfirmedDetailsDto?> GetBookingConfirmedDetailsAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Where(b => b.Id == bookingId)
            .Select(b => new BookingConfirmedDetailsDto
            {
                Id = b.Id,
                PnrCode = b.PnrCode,
                ContactEmail = b.ContactEmail,
                FlightId = b.Tickets.Select(t => (Guid?)t.FlightId).FirstOrDefault()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static BookingDto MapToDto(Booking b) => new()
    {
        Id = b.Id,
        FlightId = Guid.Empty, // flight is referenced per-ticket, not on the booking row
        UserId = b.UserId == Guid.Empty ? null : b.UserId,
        PnrCode = b.PnrCode,
        TotalPrice = b.TotalPrice,
        Status = b.Status.ToString(),
        ContactEmail = b.ContactEmail,
        ContactPhone = b.ContactPhone
    };

    private static BookingStatus ParseStatus(string status) =>
        Enum.TryParse<BookingStatus>(status, ignoreCase: true, out var parsed)
            ? parsed
            : BookingStatus.Pending;
}

public class TicketRepository : ITicketRepository
{
    private readonly BookingDbContext _context;

    public TicketRepository(BookingDbContext context)
    {
        _context = context;
    }

    public async Task<TicketDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ticket = await _context.Tickets
            .AsNoTracking()
            .Include(t => t.Passenger)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (ticket is null) return null;

        return new TicketDto
        {
            TicketId = ticket.Id,
            BookingId = ticket.BookingId,
            PassengerName = ticket.Passenger is null
                ? string.Empty
                : $"{ticket.Passenger.FirstName} {ticket.Passenger.LastName}".Trim(),
            SeatNumber = ticket.TicketNumber
        };
    }

    public async Task<Guid> CreateAsync(TicketDto ticketDto, CancellationToken cancellationToken = default)
    {
        var ticket = new Ticket
        {
            Id = ticketDto.TicketId == Guid.Empty ? Guid.NewGuid() : ticketDto.TicketId,
            BookingId = ticketDto.BookingId,
            TicketNumber = ticketDto.SeatNumber,
            Status = TicketStatus.Issued
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync(cancellationToken);
        return ticket.Id;
    }
}
