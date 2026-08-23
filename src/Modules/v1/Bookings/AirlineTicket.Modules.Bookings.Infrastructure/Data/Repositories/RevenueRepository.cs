using Microsoft.EntityFrameworkCore;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Flights.Domain.Entities;
using AirlineTicket.Modules.Flights.Infrastructure.Data;
using AirlineTicket.Modules.Bookings.Domain.Enums;

namespace AirlineTicket.Modules.Bookings.Infrastructure.Data.Repositories;

public class RevenueRepository : IRevenueRepository
{
    private readonly BookingDbContext _context;
    private readonly FlightDbContext _flightContext;

    public RevenueRepository(BookingDbContext context, FlightDbContext flightContext)
    {
        _context = context;
        _flightContext = flightContext;
    }

    public async Task<DailySalesSummary?> GetSalesSummaryAsync(Guid airlineId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var targetFlightIds = await (from f in _flightContext.Flights
                                     join r in _flightContext.Routes on f.RouteId equals r.Id
                                     where r.AirlineId == airlineId
                                     select f.Id).ToListAsync(cancellationToken);

        if (!targetFlightIds.Any())
            return new DailySalesSummary(0, 0m, 0m);

        var query = from b in _context.Bookings
                    join t in _context.Tickets on b.Id equals t.BookingId
                    where targetFlightIds.Contains(t.FlightId)
                          && b.CreatedAt >= fromDate
                          && b.CreatedAt <= toDate
                          && b.Status == BookingStatus.Confirmed
                    select b;

        var result = await query
            .Distinct()
            .GroupBy(b => 1)
            .Select(g => new DailySalesSummary(
                g.Count(),
                g.Sum(b => b.TotalPrice),
                g.Average(b => b.TotalPrice)
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return result ?? new DailySalesSummary(0, 0m, 0m);
    }

    public async Task<List<DailyOccupancy>> GetOccupancyRatesAsync(Guid airlineId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var flights = await (from f in _flightContext.Flights
                             join r in _flightContext.Routes on f.RouteId equals r.Id
                             where r.AirlineId == airlineId && f.DepartureTime >= fromDate && f.DepartureTime < toDate
                             select new
                             {
                                 f.Id,
                                 f.DepartureTime,
                                 f.FlightNumber,
                                 FlightSeatsCount = _flightContext.FlightSeats.Count(fs => fs.FlightId == f.Id)
                             }).ToListAsync(cancellationToken);

        if (!flights.Any())
            return new List<DailyOccupancy>();

        var flightIds = flights.Select(f => f.Id).ToList();
        var ticketsPerFlight = await _context.Tickets
            .Where(t => flightIds.Contains(t.FlightId))
            .GroupBy(t => t.FlightId)
            .Select(g => new { FlightId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.FlightId, x => x.Count, cancellationToken);

        var result = flights
            .GroupBy(x => new { Date = x.DepartureTime.Date, x.FlightNumber })
            .Select(g =>
            {
                var totalSeats = g.Sum(x => x.FlightSeatsCount);
                var totalTickets = g.Sum(x => ticketsPerFlight.GetValueOrDefault(x.Id, 0));
                return new DailyOccupancy(
                    g.Key.Date,
                    g.Key.FlightNumber,
                    totalSeats == 0 ? 0m : (decimal)totalTickets * 100m / totalSeats
                );
            })
            .ToList();

        return result;
    }

    public async Task<List<DailyRevenue>> GetRevenueTrendsAsync(Guid airlineId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var targetFlightIds = await (from f in _flightContext.Flights
                                     join r in _flightContext.Routes on f.RouteId equals r.Id
                                     where r.AirlineId == airlineId
                                     select f.Id).ToListAsync(cancellationToken);

        if (!targetFlightIds.Any())
            return new List<DailyRevenue>();

        var query = from b in _context.Bookings
                    join t in _context.Tickets on b.Id equals t.BookingId
                    where targetFlightIds.Contains(t.FlightId)
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
