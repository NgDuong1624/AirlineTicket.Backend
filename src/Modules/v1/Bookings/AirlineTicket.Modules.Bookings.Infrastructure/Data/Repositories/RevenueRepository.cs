using Microsoft.EntityFrameworkCore;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Flights.Domain.Entities;
using AirlineTicket.Modules.Bookings.Domain.Enums;

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
        var query = from b in _context.Bookings
                    join t in _context.Tickets on b.Id equals t.BookingId
                    join f in _context.Set<Flight>() on t.FlightId equals f.Id
                    join r in _context.Set<Route>() on f.RouteId equals r.Id
                    where r.AirlineId == airlineId && b.CreatedAt >= fromDate && b.CreatedAt <= toDate && b.Status == BookingStatus.Confirmed
                    select b;

        var result = await query
            .Distinct()
            .GroupBy(b => 1) // Group by a constant to aggregate all results
            .Select(g => new DailySalesSummary(
                g.Count(),
                g.Sum(b => b.TotalPrice),
                g.Average(b => b.TotalPrice)
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return result;
    }

    public async Task<List<DailyOccupancy>> GetOccupancyRatesAsync(Guid airlineId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var query = from f in _context.Set<Flight>()
                    join r in _context.Set<Route>() on f.RouteId equals r.Id
                    where r.AirlineId == airlineId && f.DepartureTime >= fromDate && f.DepartureTime < toDate
                    select new
                    {
                        f.DepartureTime,
                        f.FlightNumber,
                        FlightSeatsCount = _context.Set<FlightSeat>().Count(fs => fs.FlightId == f.Id),
                        TicketsCount = _context.Tickets.Count(t => t.FlightId == f.Id)
                    };

        var result = await query
            .GroupBy(x => new { Date = x.DepartureTime.Date, x.FlightNumber })
            .Select(g => new DailyOccupancy(
                g.Key.Date,
                g.Key.FlightNumber,
                g.Sum(x => x.FlightSeatsCount) == 0 ? 0m : (decimal)g.Sum(x => x.TicketsCount) * 100m / g.Sum(x => x.FlightSeatsCount)
            ))
            .ToListAsync(cancellationToken);

        return result;
    }

    public async Task<List<DailyRevenue>> GetRevenueTrendsAsync(Guid airlineId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var query = from b in _context.Bookings
                    join t in _context.Tickets on b.Id equals t.BookingId
                    join f in _context.Set<Flight>() on t.FlightId equals f.Id
                    join r in _context.Set<Route>() on f.RouteId equals r.Id
                    where r.AirlineId == airlineId
                          && b.Status == BookingStatus.Confirmed
                          && b.CreatedAt >= fromDate
                          && b.CreatedAt <= toDate
                    group b by b.CreatedAt.Date into g
                    orderby g.Key
                    select new DailyRevenue(
                        g.Key,
                        g.Distinct().Sum(b => b.TotalPrice)
                    );

        return await query.ToListAsync(cancellationToken);
    }
}