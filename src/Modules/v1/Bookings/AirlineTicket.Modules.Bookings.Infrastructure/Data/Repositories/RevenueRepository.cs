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
            SELECT COUNT(*) as TotalBookings, ISNULL(SUM(TotalPrice), 0) as TotalRevenue, ISNULL(AVG(TotalPrice), 0) as AvgTicketPrice
            FROM bookings.Bookings b
            JOIN flights.Flights f ON b.FlightId = f.Id
            JOIN flights.Routes r ON f.RouteId = r.Id
            WHERE r.AirlineId = @AirlineId AND b.CreatedAt >= @FromDate AND b.CreatedAt <= @ToDate
              AND b.Status = 'Confirmed'";

        return await connection.QueryFirstOrDefaultAsync<DailySalesSummary>(sql, new { AirlineId = airlineId, FromDate = fromDate, ToDate = toDate });
    }

    public async Task<List<DailyOccupancy>> GetOccupancyRatesAsync(Guid airlineId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        const string sql = @"
            SELECT CAST(f.DepartureTime AS DATE) as Date, f.FlightNumber,
                   CAST(COUNT(t.SeatId) AS FLOAT) / NULLIF(fs.TotalSeats, 0) * 100 as OccupancyPercent
            FROM flights.Flights f
            JOIN flights.Routes r ON f.RouteId = r.Id
            LEFT JOIN bookings.Tickets t ON t.FlightId = f.Id
            LEFT JOIN (SELECT FlightId, COUNT(*) as TotalSeats FROM flights.FlightSeats GROUP BY FlightId) fs ON fs.FlightId = f.Id
            WHERE r.AirlineId = @AirlineId AND f.DepartureTime >= @FromDate AND f.DepartureTime < @ToDate
            GROUP BY CAST(f.DepartureTime AS DATE), f.FlightNumber, fs.TotalSeats";

        var result = await connection.QueryAsync<DailyOccupancy>(sql, new { AirlineId = airlineId, FromDate = fromDate, ToDate = toDate });
        return result.ToList();
    }

    public async Task<List<DailyRevenue>> GetRevenueTrendsAsync(Guid airlineId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        const string sql = @"
            SELECT CAST(CreatedAt AS DATE) as Date, SUM(TotalPrice) as Revenue
            FROM bookings.Bookings b
            JOIN flights.Flights f ON b.FlightId = f.Id
            JOIN flights.Routes r ON f.RouteId = r.Id
            WHERE r.AirlineId = @AirlineId AND b.Status = 'Confirmed' AND b.CreatedAt >= @FromDate AND b.CreatedAt <= @ToDate
            GROUP BY CAST(CreatedAt AS DATE) ORDER BY Date";

        var result = await connection.QueryAsync<DailyRevenue>(sql, new { AirlineId = airlineId, FromDate = fromDate, ToDate = toDate });
        return result.ToList();
    }
}