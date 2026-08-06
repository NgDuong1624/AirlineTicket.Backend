using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Bookings.Domain.Entities;
using AirlineTicket.Modules.Flights.Domain.Entities;
using AirlineTicket.Modules.Flights.Domain.Enums;
using AirlineTicket.Modules.Bookings.Domain.Enums;
using AirlineTicket.Modules.Logs.Domain.Entities;
using AirlineTicket.Modules.CMS.Application.Contracts;
using AirlineTicket.Modules.CMS.Application.Features.Dashboard;
using AirlineTicket.Modules.Users.Domain.Entities;
using AirlineTicket.Modules.Users.Domain.Enums;

namespace AirlineTicket.Modules.CMS.Infrastructure.Data.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly CMSDbContext _context;

    public DashboardRepository(CMSDbContext context)
    {
        _context = context;
    }

    public async Task<AdminDashboardStatsDto?> GetAdminStatsAsync(CancellationToken cancellationToken = default)
    {
        var totalRevenue = await _context.Set<Booking>()
            .Where(b => b.Status == BookingStatus.Confirmed)
            .SumAsync(b => b.TotalPrice, cancellationToken);

        var totalBookings = await _context.Set<Booking>()
            .Where(b => b.Status == BookingStatus.Confirmed)
            .CountAsync(cancellationToken);

        var newUsers = await _context.Set<User>()
            .Where(
                u => u.Role == (int)UserRole.Customer &&
                u.CreatedAt >= DateTime.UtcNow.AddDays(-30))
            .CountAsync(cancellationToken);

        var totalFlights = await _context.Set<Flight>()
            .Where(f => f.DepartureTime >= DateTime.UtcNow.AddDays(-30))
            .CountAsync(cancellationToken);

        return new AdminDashboardStatsDto(totalRevenue, totalBookings, newUsers, totalFlights);
    }

    public async Task<List<AdminDashboardPartnerDto>> GetRecentPartnersAsync(int count = 5, CancellationToken cancellationToken = default)
    {
        var partners = await _context.Set<Airline>()
            .Where(a => !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .Select(a => new AdminDashboardPartnerDto(
                a.Name,
                a.IataCode,
                _context.Set<Flight>().Count(f => _context.Set<Route>().Any(r => r.Id == f.RouteId && r.AirlineId == a.Id)),
                a.IsActive ? "Active" : "Inactive",
                a.CreatedAt.ToString("yyyy-MM-dd")
            ))
            .ToListAsync(cancellationToken);

        return partners;
    }

    public async Task<List<AdminDashboardLogDto>> GetCriticalLogsAsync(int count = 4, CancellationToken cancellationToken = default)
    {
        var logs = await _context.Set<SystemLog>()
            .OrderByDescending(l => l.CreatedAt)
            .Take(count)
            .Select(l => new AdminDashboardLogDto(
                l.CreatedAt.ToString("HH:mm:ss"),
                l.Level,
                l.Message
            ))
            .ToListAsync(cancellationToken);

        return logs;
    }

    public async Task<List<PartnerStatDto>> GetPartnerStatsAsync(Guid airlineId, CancellationToken cancellationToken = default)
    {
        var activeAircraft = await _context.Set<Airplane>()
            .Where(a => a.AirlineId == airlineId && !a.IsDeleted)
            .CountAsync(cancellationToken);

        var totalStaff = await _context.Set<User>()
            .Where(u => u.AirlineId == airlineId && u.IsActive)
            .CountAsync(cancellationToken);

        var todayBookings = await _context.Set<Booking>()
            .Where(b => b.Status == BookingStatus.Confirmed && b.CreatedAt.Date == DateTime.UtcNow.Date)
            .Where(b => _context.Set<Ticket>()
                .Any(t => t.BookingId == b.Id && _context.Set<Flight>()
                    .Any(f => f.Id == t.FlightId && _context.Set<Route>().Any(r => r.Id == f.RouteId && r.AirlineId == airlineId))))
            .CountAsync(cancellationToken);

        var monthlyRevenue = await _context.Set<Booking>()
            .Where(b => b.Status == BookingStatus.Confirmed && b.CreatedAt >= DateTime.UtcNow.AddDays(-30))
            .Where(b => _context.Set<Ticket>()
                .Any(t => t.BookingId == b.Id && _context.Set<Flight>()
                    .Any(f => f.Id == t.FlightId && f.Route.AirlineId == airlineId)))
            .SumAsync(b => b.TotalPrice, cancellationToken);

        return new List<PartnerStatDto>
        {
            new(activeAircraft.ToString()),
            new(totalStaff.ToString()),
            new(todayBookings.ToString()),
            new($"${monthlyRevenue:N0}")
        };
    }

    public async Task<List<PartnerRecentFlightDto>> GetPartnerRecentFlightsAsync(Guid airlineId, int count = 5, CancellationToken cancellationToken = default)
    {
        var flights = await _context.Set<Flight>()
            .Where(f => f.Route.AirlineId == airlineId)
            .OrderByDescending(f => f.DepartureTime)
            .Take(count)
            .Select(f => new PartnerRecentFlightDto(
                f.FlightNumber,
                f.Route.OriginAirport.IataCode + "-" + f.Route.DestinationAirport.IataCode,
                f.DepartureTime.ToString("HH:mm:ss"),
                f.Status == FlightStatus.Scheduled ? "Scheduled" :
                f.Status == FlightStatus.Delayed ? "Delayed" :
                f.Status == FlightStatus.Boarding ? "Boarding" :
                f.Status == FlightStatus.InAir ? "InAir" :
                f.Status == FlightStatus.Landed ? "Landed" :
                f.Status == FlightStatus.Cancelled ? "Cancelled" : "Unknown",
                f.Status == FlightStatus.Scheduled ? "text-sky-600 bg-sky-500/10" :
                f.Status == FlightStatus.Delayed ? "text-amber-600 bg-amber-500/10" :
                f.Status == FlightStatus.Boarding ? "text-emerald-600 bg-emerald-500/10" :
                f.Status == FlightStatus.InAir ? "text-indigo-600 bg-indigo-500/10" :
                f.Status == FlightStatus.Landed ? "text-gray-600 bg-gray-500/10" :
                f.Status == FlightStatus.Cancelled ? "text-rose-600 bg-rose-500/10" : "text-muted-foreground bg-muted"
            ))
            .ToListAsync(cancellationToken);

        return flights;
    }

    public async Task<List<PartnerRecentBookingDto>> GetPartnerRecentBookingsAsync(Guid airlineId, int count = 5, CancellationToken cancellationToken = default)
    {
        var bookings = await (from b in _context.Set<Booking>()
                              join t in _context.Set<Ticket>() on b.Id equals t.BookingId
                              join fs in _context.Set<FlightSeat>() on t.SeatId equals fs.Id
                              join f in _context.Set<Flight>() on t.FlightId equals f.Id
                              join r in _context.Set<Route>() on f.RouteId equals r.Id
                              where r.AirlineId == airlineId
                              orderby b.CreatedAt descending
                              select new
                              {
                                  b.PnrCode,
                                  PassengerName = _context.Set<Passenger>()
                                      .Where(p => p.BookingId == b.Id)
                                      .Select(p => p.LastName + " " + p.FirstName)
                                      .FirstOrDefault() ?? string.Empty,
                                  f.FlightNumber,
                                  fs.SeatNumber,
                                  b.TotalPrice,
                                  b.CreatedAt
                              })
                              .Take(count)
                              .ToListAsync(cancellationToken);

        return bookings.Select(b => new PartnerRecentBookingDto(
            b.PnrCode,
            b.PassengerName,
            b.FlightNumber,
            b.SeatNumber,
            "Economy",
            "$" + b.TotalPrice.ToString(),
            (DateTime.UtcNow - b.CreatedAt).TotalMinutes < 60
                ? Math.Floor((DateTime.UtcNow - b.CreatedAt).TotalMinutes).ToString() + " mins ago"
                : b.CreatedAt.ToString("dd/MM/yyyy")
        )).ToList();
    }
}

public class PartnerStatsQueryResult
{
    public int ActiveAircraft { get; set; }
    public int TotalStaff { get; set; }
    public int TodayBookings { get; set; }
    public decimal MonthlyRevenue { get; set; }
}

