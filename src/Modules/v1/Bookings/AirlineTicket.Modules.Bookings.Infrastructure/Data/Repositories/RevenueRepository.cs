using Dapper;
using Microsoft.EntityFrameworkCore;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Bookings.Infrastructure.Data.Repositories;

public class RevenueRepository : IRevenueRepository
{
    private readonly BookingDbContext _context;

    public RevenueRepository(BookingDbContext context)
    {
        _context = context;
    }

    public async Task<DailySalesSummary?> GetSalesSummaryAsync(Guid airlineId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        const string sql = @"
            SELECT COUNT(*) as TotalBookings, COALESCE(SUM(total_price), 0) as TotalRevenue, COALESCE(AVG(total_price), 0) as AvgTicketPrice
            FROM bookings.bookings b
            JOIN flights.flights f ON b.flight_id = f.id
            JOIN flights.routes r ON f.route_id = r.id
            WHERE r.airline_id = @AirlineId AND b.created_at >= @FromDate AND b.created_at <= @ToDate
              AND b.status = 'Confirmed'";

        return await connection.QueryFirstOrDefaultAsync<DailySalesSummary>(sql, new { AirlineId = airlineId, FromDate = fromDate, ToDate = toDate });
    }

    public async Task<List<DailyOccupancy>> GetOccupancyRatesAsync(Guid airlineId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        const string sql = @"
            SELECT f.departure_time::date as Date, f.flight_number,
                   COUNT(t.seat_id)::float / NULLIF(fs.total_seats, 0) * 100 as OccupancyPercent
            FROM flights.flights f
            JOIN flights.routes r ON f.route_id = r.id
            LEFT JOIN bookings.tickets t ON t.flight_id = f.id
            LEFT JOIN (SELECT flight_id, COUNT(*) as total_seats FROM flights.flight_seats GROUP BY flight_id) fs ON fs.flight_id = f.id
            WHERE r.airline_id = @AirlineId AND f.departure_time >= @FromDate AND f.departure_time < @ToDate
            GROUP BY f.departure_time::date, f.flight_number, fs.total_seats";

        var result = await connection.QueryAsync<DailyOccupancy>(sql, new { AirlineId = airlineId, FromDate = fromDate, ToDate = toDate });
        return result.ToList();
    }

    public async Task<List<DailyRevenue>> GetRevenueTrendsAsync(Guid airlineId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        const string sql = @"
            SELECT created_at::date as Date, SUM(total_price) as Revenue
            FROM bookings.bookings b
            JOIN flights.flights f ON b.flight_id = f.id
            JOIN flights.routes r ON f.route_id = r.id
            WHERE r.airline_id = @AirlineId AND b.status = 'Confirmed' AND b.created_at >= @FromDate AND b.created_at <= @ToDate
            GROUP BY created_at::date ORDER BY Date";

        var result = await connection.QueryAsync<DailyRevenue>(sql, new { AirlineId = airlineId, FromDate = fromDate, ToDate = toDate });
        return result.ToList();
    }
}