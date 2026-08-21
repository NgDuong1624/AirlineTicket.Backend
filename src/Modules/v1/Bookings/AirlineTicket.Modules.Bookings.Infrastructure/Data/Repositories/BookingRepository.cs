using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Domain.Entities;
using AirlineTicket.Modules.Bookings.Domain.Enums;
using Microsoft.EntityFrameworkCore;

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
        return await _context.Bookings
            .AsNoTracking()
            .Where(b => b.Id == id)
            .Select(b => new BookingDto
            {
                Id = b.Id,
                UserId = b.UserId,
                PnrCode = b.PnrCode,
                TotalPrice = b.TotalPrice,
                Status = b.Status.ToString(),
                ContactEmail = b.ContactEmail,
                ContactPhone = b.ContactPhone,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<(List<BookingDto> Items, int TotalCount)> GetByUserIdAsync(Guid userId, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Bookings
            .AsNoTracking()
            .Where(b => b.UserId == userId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BookingDto
            {
                Id = b.Id,
                UserId = b.UserId,
                PnrCode = b.PnrCode,
                TotalPrice = b.TotalPrice,
                Status = b.Status.ToString(),
                ContactEmail = b.ContactEmail,
                ContactPhone = b.ContactPhone,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<UserBookingStatsDto> GetStatsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var bookings = await _context.Bookings
            .AsNoTracking()
            .Where(b => b.UserId == userId && b.Status != BookingStatus.Cancelled)
            .Select(b => new { b.TotalPrice, b.CreatedAt })
            .ToListAsync(cancellationToken);

        var totalBookings = bookings.Count;
        var totalSpent = bookings.Sum(b => b.TotalPrice);

        var now = DateTime.UtcNow;
        var firstDayCurrentMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var firstDayLastMonth = firstDayCurrentMonth.AddMonths(-1);
        var firstDayCurrentYear = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var firstDayLastYear = firstDayCurrentYear.AddYears(-1);

        var lastMonthSpent = bookings
            .Where(b => b.CreatedAt >= firstDayLastMonth && b.CreatedAt < firstDayCurrentMonth)
            .Sum(b => b.TotalPrice);

        var lastYearSpent = bookings
            .Where(b => b.CreatedAt >= firstDayLastYear && b.CreatedAt < firstDayCurrentYear)
            .Sum(b => b.TotalPrice);

        return new UserBookingStatsDto(totalBookings, totalSpent, lastMonthSpent, lastYearSpent);
    }

    public async Task<(List<BookingDto> Items, int TotalCount)> GetAllAsync(
        int pageIndex, 
        int pageSize, 
        string? search,
        string? status, 
        DateTime? date, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.Bookings.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(b => b.PnrCode.Contains(search)
                || b.Passengers.Any(p => (p.FirstName + " " + p.LastName).Contains(search)));

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(b => b.Status.ToString() == status);

        if (date.HasValue)
            query = query.Where(b => b.CreatedAt.Date == date.Value.Date);

        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BookingDto
            {
                Id = b.Id,
                UserId = b.UserId,
                PnrCode = b.PnrCode,
                TotalPrice = b.TotalPrice,
                Status = b.Status.ToString(),
                ContactEmail = b.ContactEmail,
                ContactPhone = b.ContactPhone,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            })
            .ToListAsync(cancellationToken);
            
        return (items, totalCount);
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
        var raw = await _context.Bookings
            .AsNoTracking()
            .Where(b => b.Id == bookingId)
            .Select(b => new
            {
                b.Id,
                b.PnrCode,
                b.ContactEmail,
                PassengerNames = b.Passengers.Select(p => p.FirstName + " " + p.LastName).ToList(),
                FlightId = b.Tickets.Select(t => (Guid?)t.FlightId).FirstOrDefault()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (raw == null) return null;

        var passengerName = raw.PassengerNames.FirstOrDefault() ?? string.Empty;
        if (raw.PassengerNames.Count > 1)
        {
            passengerName += $" and {raw.PassengerNames.Count - 1} others";
        }

        return new BookingConfirmedDetailsDto
        {
            Id = raw.Id,
            PnrCode = raw.PnrCode,
            ContactEmail = raw.ContactEmail,
            PassengerName = passengerName,
            FlightId = raw.FlightId
        };
    }

    private static BookingDto MapToDto(Booking b) => new()
    {
        Id = b.Id,
        FlightId = Guid.Empty,
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
