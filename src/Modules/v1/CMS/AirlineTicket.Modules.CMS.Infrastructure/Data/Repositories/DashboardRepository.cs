using Dapper;
using Microsoft.EntityFrameworkCore;
using AirlineTicket.Modules.CMS.Application.Contracts;
using AirlineTicket.Modules.CMS.Application.Features.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        var connection = _context.Database.GetDbConnection();
        const string sql = @"
            SELECT
                (SELECT ISNULL(SUM(TotalPrice), 0) FROM dbo.Bookings WHERE Status = 1) as TotalRevenue,
                (SELECT COUNT(*) FROM dbo.Bookings WHERE Status = 1) as TotalBookings,
                (SELECT COUNT(*) FROM dbo.Users WHERE CreatedAt >= DATEADD(day, -30, GETUTCDATE())) as NewUsers,
                (SELECT COUNT(*) FROM dbo.Flights WHERE DepartureTime >= DATEADD(day, -30, GETUTCDATE())) as TotalFlights";
        return await connection.QueryFirstOrDefaultAsync<AdminDashboardStatsDto>(sql);
    }

    public async Task<List<AdminDashboardPartnerDto>> GetRecentPartnersAsync(int count = 5, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        const string sql = @"
            SELECT TOP (@Count)
                a.Name,
                a.IataCode as Code,
                (SELECT COUNT(*) FROM dbo.Flights f JOIN dbo.Routes r ON f.RouteId = r.Id WHERE r.AirlineId = a.Id) as FlightsCount,
                CASE WHEN a.IsActive = 1 THEN 'Active' ELSE 'Inactive' END as Status,
                CONVERT(varchar, a.CreatedAt, 23) as Joined
            FROM dbo.Airlines a
            WHERE a.IsDeleted = 0
            ORDER BY a.CreatedAt DESC";
        var result = await connection.QueryAsync<AdminDashboardPartnerDto>(sql, new { Count = count });
        return result.ToList();
    }

    public async Task<List<AdminDashboardLogDto>> GetCriticalLogsAsync(int count = 4, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        const string sql = @"
            SELECT TOP (@Count)
                CONVERT(varchar, CreatedAt, 108) as Time,
                Level as Type,
                Message as LabelKey
            FROM dbo.SystemLogs
            ORDER BY CreatedAt DESC";
        var result = await connection.QueryAsync<AdminDashboardLogDto>(sql, new { Count = count });
        return result.ToList();
    }

    public async Task<List<PartnerStatDto>> GetPartnerStatsAsync(Guid airlineId, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        const string sql = @"
            SELECT
                (SELECT COUNT(*) FROM dbo.Airplanes WHERE AirlineId = @AirlineId AND IsDeleted = 0) as ActiveAircraft,
                (SELECT COUNT(*) FROM dbo.Users WHERE AirlineId = @AirlineId AND IsActive = 1) as TotalStaff,
                (SELECT COUNT(*) FROM dbo.Bookings b
                    WHERE b.Status = 1 AND CAST(b.CreatedAt AS DATE) = CAST(GETUTCDATE() AS DATE)
                      AND EXISTS (
                          SELECT 1 FROM dbo.Tickets t
                          INNER JOIN dbo.Flights f ON t.FlightId = f.Id
                          INNER JOIN dbo.Routes r ON f.RouteId = r.Id
                          WHERE t.BookingId = b.Id AND r.AirlineId = @AirlineId
                      )
                ) as TodayBookings,
                ISNULL((
                    SELECT SUM(b.TotalPrice) FROM dbo.Bookings b
                    WHERE b.Status = 1 AND b.CreatedAt >= DATEADD(day, -30, GETUTCDATE())
                      AND EXISTS (
                          SELECT 1 FROM dbo.Tickets t
                          INNER JOIN dbo.Flights f ON t.FlightId = f.Id
                          INNER JOIN dbo.Routes r ON f.RouteId = r.Id
                          WHERE t.BookingId = b.Id AND r.AirlineId = @AirlineId
                      )
                ), 0) as MonthlyRevenue";

        var result = await connection.QueryFirstOrDefaultAsync<PartnerStatsQueryResult>(sql, new { AirlineId = airlineId });
        if (result == null) return new List<PartnerStatDto>();

        return new List<PartnerStatDto>
        {
            new(result.ActiveAircraft.ToString()),
            new(result.TotalStaff.ToString()),
            new(result.TodayBookings.ToString()),
            new($"${result.MonthlyRevenue:N0}")
        };
    }

    public async Task<List<PartnerRecentFlightDto>> GetPartnerRecentFlightsAsync(Guid airlineId, int count = 5, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        const string sql = @"
            SELECT TOP (@Count)
                f.FlightNumber as Id,
                (SELECT IataCode FROM dbo.Airports WHERE Id = r.OriginAirportId) + '-' + (SELECT IataCode FROM dbo.Airports WHERE Id = r.DestinationAirportId) as Route,
                CONVERT(varchar, f.DepartureTime, 108) as Time,
                CASE f.Status
                    WHEN 0 THEN 'Scheduled'
                    WHEN 1 THEN 'Delayed'
                    WHEN 2 THEN 'Boarding'
                    WHEN 3 THEN 'InAir'
                    WHEN 4 THEN 'Landed'
                    WHEN 5 THEN 'Cancelled'
                    ELSE 'Unknown'
                END as Status,
                CASE f.Status
                    WHEN 0 THEN 'text-sky-600 bg-sky-500/10'
                    WHEN 1 THEN 'text-amber-600 bg-amber-500/10'
                    WHEN 2 THEN 'text-emerald-600 bg-emerald-500/10'
                    WHEN 3 THEN 'text-indigo-600 bg-indigo-500/10'
                    WHEN 4 THEN 'text-gray-600 bg-gray-500/10'
                    WHEN 5 THEN 'text-rose-600 bg-rose-500/10'
                    ELSE 'text-muted-foreground bg-muted'
                END as Color
            FROM dbo.Flights f
            INNER JOIN dbo.Routes r ON f.RouteId = r.Id
            WHERE r.AirlineId = @AirlineId
            ORDER BY f.DepartureTime DESC";

        var result = await connection.QueryAsync<PartnerRecentFlightDto>(sql, new { AirlineId = airlineId, Count = count });
        return result.ToList();
    }

    public async Task<List<PartnerRecentBookingDto>> GetPartnerRecentBookingsAsync(Guid airlineId, int count = 5, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        const string sql = @"
            SELECT TOP (@Count)
                b.PnrCode as Id,
                (SELECT TOP 1 (p.LastName + ' ' + p.FirstName) FROM dbo.Passengers p WHERE p.BookingId = b.Id) as Passenger,
                f.FlightNumber as Flight,
                fs.SeatNumber as Seat,
                'Economy' as Class,
                '$' + CAST(b.TotalPrice AS varchar) as Amount,
                CASE
                    WHEN DATEDIFF(MINUTE, b.CreatedAt, GETUTCDATE()) < 60
                    THEN CAST(DATEDIFF(MINUTE, b.CreatedAt, GETUTCDATE()) AS varchar) + ' mins ago'
                    ELSE CONVERT(varchar, b.CreatedAt, 103)
                END as Date
            FROM dbo.Bookings b
            INNER JOIN dbo.Tickets t ON b.Id = t.BookingId
            INNER JOIN dbo.FlightSeats fs ON t.SeatId = fs.Id
            INNER JOIN dbo.Flights f ON t.FlightId = f.Id
            INNER JOIN dbo.Routes r ON f.RouteId = r.Id
            WHERE r.AirlineId = @AirlineId
            ORDER BY b.CreatedAt DESC";

        var result = await connection.QueryAsync<PartnerRecentBookingDto>(sql, new { AirlineId = airlineId, Count = count });
        return result.ToList();
    }
}

public class PartnerStatsQueryResult
{
    public int ActiveAircraft { get; set; }
    public int TotalStaff { get; set; }
    public int TodayBookings { get; set; }
    public decimal MonthlyRevenue { get; set; }
}

