using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Bookings.Application.Contracts;

public record DailySalesSummary(int TotalBookings, decimal TotalRevenue, decimal AvgTicketPrice);
public record DailyOccupancy(DateTime Date, string FlightNumber, decimal OccupancyPercent);
public record DailyRevenue(DateTime Date, decimal Revenue);

public interface IRevenueRepository
{
    Task<DailySalesSummary?> GetSalesSummaryAsync(Guid airlineId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task<List<DailyOccupancy>> GetOccupancyRatesAsync(Guid airlineId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task<List<DailyRevenue>> GetRevenueTrendsAsync(Guid airlineId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
}
