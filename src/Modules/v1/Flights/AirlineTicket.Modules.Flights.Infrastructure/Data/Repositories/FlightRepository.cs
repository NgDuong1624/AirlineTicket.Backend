using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;

namespace AirlineTicket.Modules.Flights.Infrastructure.Data.Repositories;

public class FlightRepository : IFlightRepository
{
    private readonly FlightDbContext _context;

    public FlightRepository(FlightDbContext context)
    {
        _context = context;
    }

    public async Task<FlightDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var flight = await _context.Flights
            .Include(f => f.Route)
            .Include(f => f.Airplane)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

        if (flight == null) return null;

        return new FlightDto
        {
            Id = flight.Id,
            RouteId = flight.RouteId,
            AirplaneId = flight.AirplaneId,
            FlightNumber = flight.FlightNumber,
            BasePrice = flight.BasePrice
        };
    }

    public async Task<List<FlightDto>> SearchAsync(
        string origin,
        string destination,
        DateTime date,
        string? cabinClass = null,
        List<string>? airlines = null,
        decimal? priceRangeMin = null,
        decimal? priceRangeMax = null,
        int? maxStops = null,
        string? sortBy = null,
        string currency = "VND",
        CancellationToken cancellationToken = default)
    {
        var query = _context.Flights
            .Include(f => f.Route)
                .ThenInclude(r => r.OriginAirport)
            .Include(f => f.Route)
                .ThenInclude(r => r.DestinationAirport)
            .Where(f => f.Route.OriginAirport.IataCode == origin
                     && f.Route.DestinationAirport.IataCode == destination
                     && f.DepartureTime.Date == date.Date);

        // Filter by airlines if provided
        if (airlines != null && airlines.Any())
        {
            query = query.Where(f => airlines.Contains(f.Route.Airline.Name));
        }

        // Filter by price
        if (priceRangeMin.HasValue)
        {
            query = query.Where(f => f.BasePrice >= priceRangeMin.Value);
        }
        if (priceRangeMax.HasValue)
        {
            query = query.Where(f => f.BasePrice <= priceRangeMax.Value);
        }

        // Sorting
        if (!string.IsNullOrEmpty(sortBy))
        {
            if (sortBy.Equals("price_asc", StringComparison.OrdinalIgnoreCase))
                query = query.OrderBy(f => f.BasePrice);
            else if (sortBy.Equals("price_desc", StringComparison.OrdinalIgnoreCase))
                query = query.OrderByDescending(f => f.BasePrice);
        }

        return await query
            .Select(f => new FlightDto
            {
                Id = f.Id,
                RouteId = f.RouteId,
                AirplaneId = f.AirplaneId,
                FlightNumber = f.FlightNumber,
                BasePrice = f.BasePrice
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid> CreateAsync(FlightDto flightDto, CancellationToken cancellationToken = default)
    {
        var flight = new Flight
        {
            Id = flightDto.Id == Guid.Empty ? Guid.NewGuid() : flightDto.Id,
            RouteId = flightDto.RouteId,
            AirplaneId = flightDto.AirplaneId,
            FlightNumber = flightDto.FlightNumber,
            BasePrice = flightDto.BasePrice,
            DepartureTime = DateTime.UtcNow.AddDays(1),
            ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(2),
            Status = AirlineTicket.Modules.Flights.Domain.Enums.FlightStatus.Scheduled
        };

        _context.Flights.Add(flight);
        await _context.SaveChangesAsync(cancellationToken);
        return flight.Id;
    }

    public async Task<List<FlightDto>> GetTrendingAsync(CancellationToken cancellationToken = default)
    {
        // Simple trending logic: take top 5 cheapest flights
        return await _context.Flights
            .OrderBy(f => f.BasePrice)
            .Take(5)
            .Select(f => new FlightDto
            {
                Id = f.Id,
                RouteId = f.RouteId,
                AirplaneId = f.AirplaneId,
                FlightNumber = f.FlightNumber,
                BasePrice = f.BasePrice
            })
            .ToListAsync(cancellationToken);
    }
}

public class AirportRepository : IAirportRepository
{
    private readonly FlightDbContext _context;

    public AirportRepository(FlightDbContext context)
    {
        _context = context;
    }

    public async Task<Airport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Airports.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<List<Airport>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Airports.ToListAsync(cancellationToken);
    }

    public async Task<List<Airport>> SearchAsync(string? search, CancellationToken cancellationToken = default)
    {
        var query = _context.Airports.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(a => a.NameEn.ToLower().Contains(search)
                                  || a.NameVi.ToLower().Contains(search)
                                  || a.IataCode.ToLower().Contains(search)
                                  || a.CityEn.ToLower().Contains(search)
                                  || a.CityVi.ToLower().Contains(search));
        }

        return await query.ToListAsync(cancellationToken);
    }
}

public class RouteRepository : IRouteRepository
{
    private readonly FlightDbContext _context;

    public RouteRepository(FlightDbContext context)
    {
        _context = context;
    }

    public async Task<List<Route>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Routes
            .Include(r => r.Airline)
            .Include(r => r.OriginAirport)
            .Include(r => r.DestinationAirport)
            .ToListAsync(cancellationToken);
    }
}

public class FlightSeatRepository : IFlightSeatRepository
{
    private readonly FlightDbContext _context;

    public FlightSeatRepository(FlightDbContext context)
    {
        _context = context;
    }

    public async Task<List<FlightSeat>> GetByFlightIdAsync(Guid flightId, CancellationToken cancellationToken = default)
    {
        return await _context.FlightSeats
            .Where(fs => fs.FlightId == flightId)
            .ToListAsync(cancellationToken);
    }
}