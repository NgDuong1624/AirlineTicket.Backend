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
                (SELECT COALESCE(SUM(total_price), 0) FROM bookings.bookings WHERE status = 1) as TotalRevenue,
                (SELECT COUNT(*)::integer FROM bookings.bookings WHERE status = 1) as TotalBookings,
                (SELECT COUNT(*)::integer FROM users.users WHERE created_at >= NOW() - INTERVAL '30 days') as NewUsers,
                (SELECT COUNT(*)::integer FROM flights.flights WHERE departure_time >= NOW() - INTERVAL '30 days') as TotalFlights";
        return await connection.QueryFirstOrDefaultAsync<AdminDashboardStatsDto>(sql);
    }

    public async Task<List<AdminDashboardPartnerDto>> GetRecentPartnersAsync(int count = 5, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        const string sql = @"
            SELECT
                a.name,
                a.iata_code as Code,
                (SELECT COUNT(*)::integer FROM flights.flights f JOIN flights.routes r ON f.route_id = r.id WHERE r.airline_id = a.id) as FlightsCount,
                CASE WHEN a.is_active = TRUE THEN 'Active' ELSE 'Inactive' END as Status,
                TO_CHAR(a.created_at, 'YYYY-MM-DD') as Joined
            FROM flights.airlines a
            WHERE a.is_deleted = FALSE
            ORDER BY a.created_at DESC
            LIMIT @Count";
        var result = await connection.QueryAsync<AdminDashboardPartnerDto>(sql, new { Count = count });
        return result.ToList();
    }

    public async Task<List<AdminDashboardLogDto>> GetCriticalLogsAsync(int count = 4, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        const string sql = @"
            SELECT
                TO_CHAR(created_at, 'HH24:MI:SS') as Time,
                level as Type,
                message as LabelKey
            FROM logs.system_logs
            ORDER BY created_at DESC
            LIMIT @Count";
        var result = await connection.QueryAsync<AdminDashboardLogDto>(sql, new { Count = count });
        return result.ToList();
    }

    public async Task<List<PartnerStatDto>> GetPartnerStatsAsync(Guid airlineId, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        const string sql = @"
            SELECT
                (SELECT COUNT(*) FROM flights.airplanes WHERE airline_id = @AirlineId AND is_deleted = FALSE) as ActiveAircraft,
                (SELECT COUNT(*) FROM users.users WHERE airline_id = @AirlineId AND is_active = TRUE) as TotalStaff,
                (SELECT COUNT(*) FROM bookings.bookings b
                    WHERE b.status = 1 AND CAST(b.created_at AS DATE) = CAST(NOW() AS DATE)
                      AND EXISTS (
                          SELECT 1 FROM bookings.tickets t
                          INNER JOIN flights.flights f ON t.flight_id = f.id
                          INNER JOIN flights.routes r ON f.route_id = r.id
                          WHERE t.booking_id = b.id AND r.airline_id = @AirlineId
                      )
                ) as TodayBookings,
                COALESCE((
                    SELECT SUM(b.total_price) FROM bookings.bookings b
                    WHERE b.status = 1 AND b.created_at >= NOW() - INTERVAL '30 days'
                      AND EXISTS (
                          SELECT 1 FROM bookings.tickets t
                          INNER JOIN flights.flights f ON t.flight_id = f.id
                          INNER JOIN flights.routes r ON f.route_id = r.id
                          WHERE t.booking_id = b.id AND r.airline_id = @AirlineId
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
            SELECT
                f.""FlightNumber"" as Id,
                (SELECT ""IataCode"" FROM flights.""Airports"" WHERE ""Id"" = r.""OriginAirportId"") || '-' || (SELECT ""IataCode"" FROM flights.""Airports"" WHERE ""Id"" = r.""DestinationAirportId"") as Route,
                TO_CHAR(f.""DepartureTime"", 'HH24:MI:SS') as Time,
                CASE f.""Status""
                    WHEN 0 THEN 'Scheduled'
                    WHEN 1 THEN 'Delayed'
                    WHEN 2 THEN 'Boarding'
                    WHEN 3 THEN 'InAir'
                    WHEN 4 THEN 'Landed'
                    WHEN 5 THEN 'Cancelled'
                    ELSE 'Unknown'
                END as Status,
                CASE f.""Status""
                    WHEN 0 THEN 'text-sky-600 bg-sky-500/10'
                    WHEN 1 THEN 'text-amber-600 bg-amber-500/10'
                    WHEN 2 THEN 'text-emerald-600 bg-emerald-500/10'
                    WHEN 3 THEN 'text-indigo-600 bg-indigo-500/10'
                    WHEN 4 THEN 'text-gray-600 bg-gray-500/10'
                    WHEN 5 THEN 'text-rose-600 bg-rose-500/10'
                    ELSE 'text-muted-foreground bg-muted'
                END as Color
            FROM flights.""Flights"" f
            INNER JOIN flights.""Routes"" r ON f.""RouteId"" = r.""Id""
            WHERE r.""AirlineId"" = @AirlineId
            ORDER BY f.""DepartureTime"" DESC
            LIMIT @Count";

        var result = await connection.QueryAsync<PartnerRecentFlightDto>(sql, new { AirlineId = airlineId, Count = count });
        return result.ToList();
    }

    public async Task<List<PartnerRecentBookingDto>> GetPartnerRecentBookingsAsync(Guid airlineId, int count = 5, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        const string sql = @"
            SELECT
                b.""PnrCode"" as Id,
                (SELECT (p.""LastName"" || ' ' || p.""FirstName"") FROM bookings.""Passengers"" p WHERE p.""BookingId"" = b.""Id"" LIMIT 1) as Passenger,
                f.""FlightNumber"" as Flight,
                fs.""SeatNumber"" as Seat,
                'Economy' as Class,
                '$' || CAST(b.""TotalPrice"" AS TEXT) as Amount,
                CASE
                    WHEN EXTRACT(EPOCH FROM (NOW() - b.""CreatedAt"")) / 60 < 60
                    THEN CAST(FLOOR(EXTRACT(EPOCH FROM (NOW() - b.""CreatedAt"")) / 60) AS TEXT) || ' mins ago'
                    ELSE TO_CHAR(b.""CreatedAt"", 'DD/MM/YYYY')
                END as Date
            FROM bookings.""Bookings"" b
            INNER JOIN bookings.""Tickets"" t ON b.""Id"" = t.""BookingId""
            INNER JOIN flights.""FlightSeats"" fs ON t.""SeatId"" = fs.""Id""
            INNER JOIN flights.""Flights"" f ON t.""FlightId"" = f.""Id""
            INNER JOIN flights.""Routes"" r ON f.""RouteId"" = r.""Id""
            WHERE r.""AirlineId"" = @AirlineId
            ORDER BY b.""CreatedAt"" DESC
            LIMIT @Count";

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

