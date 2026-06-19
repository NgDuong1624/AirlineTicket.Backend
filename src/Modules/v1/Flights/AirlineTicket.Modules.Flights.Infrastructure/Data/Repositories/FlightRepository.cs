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

    public async Task<Guid> CreateAsync(Airport airport, CancellationToken cancellationToken = default)
    {
        if (airport.Id == Guid.Empty)
            airport.Id = Guid.NewGuid();

        _context.Airports.Add(airport);
        await _context.SaveChangesAsync(cancellationToken);
        return airport.Id;
    }

    public async Task UpdateAsync(Airport airport, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Airports.FirstOrDefaultAsync(a => a.Id == airport.Id, cancellationToken);
        if (existing is null) return;

        existing.IataCode = airport.IataCode;
        existing.NameEn = airport.NameEn;
        existing.NameVi = airport.NameVi;
        existing.CityEn = airport.CityEn;
        existing.CityVi = airport.CityVi;
        existing.CountryCode = airport.CountryCode;
        existing.Timezone = airport.Timezone;
        existing.IsActive = airport.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Airports.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (existing is null) return;

        existing.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
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

    public async Task<List<Route>> GetByAirlineAsync(Guid airlineId, CancellationToken cancellationToken = default)
    {
        return await _context.Routes
            .Where(r => r.AirlineId == airlineId && !r.IsDeleted)
            .Include(r => r.OriginAirport)
            .Include(r => r.DestinationAirport)
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid> CreateAsync(Route route, CancellationToken cancellationToken = default)
    {
        if (route.Id == Guid.Empty)
            route.Id = Guid.NewGuid();

        _context.Routes.Add(route);
        await _context.SaveChangesAsync(cancellationToken);
        return route.Id;
    }

    public async Task UpdateAsync(Route route, Guid airlineId, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Routes
            .FirstOrDefaultAsync(r => r.Id == route.Id && r.AirlineId == airlineId, cancellationToken);
        if (existing is null) return;

        existing.OriginAirportId = route.OriginAirportId;
        existing.DestinationAirportId = route.DestinationAirportId;
        existing.DistanceKm = route.DistanceKm;
        existing.EstimatedDurationMinutes = route.EstimatedDurationMinutes;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, Guid airlineId, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Routes
            .FirstOrDefaultAsync(r => r.Id == id && r.AirlineId == airlineId, cancellationToken);
        if (existing is null) return;

        existing.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class AirplaneRepository : IAirplaneRepository
{
    private readonly FlightDbContext _context;

    public AirplaneRepository(FlightDbContext context)
    {
        _context = context;
    }

    public async Task<List<Airplane>> GetByAirlineAsync(Guid airlineId, CancellationToken cancellationToken = default)
    {
        return await _context.Airplanes
            .Where(a => a.AirlineId == airlineId && !a.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid> CreateAsync(Airplane airplane, CancellationToken cancellationToken = default)
    {
        if (airplane.Id == Guid.Empty)
            airplane.Id = Guid.NewGuid();

        _context.Airplanes.Add(airplane);
        await _context.SaveChangesAsync(cancellationToken);
        return airplane.Id;
    }

    public async Task UpdateAsync(Airplane airplane, Guid airlineId, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Airplanes
            .FirstOrDefaultAsync(a => a.Id == airplane.Id && a.AirlineId == airlineId, cancellationToken);
        if (existing is null) return;

        existing.Model = airplane.Model;
        existing.RegistrationNumber = airplane.RegistrationNumber;
        existing.TotalCapacity = airplane.TotalCapacity;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, Guid airlineId, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Airplanes
            .FirstOrDefaultAsync(a => a.Id == id && a.AirlineId == airlineId, cancellationToken);
        if (existing is null) return;

        existing.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
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
public class AirlineRepository : IAirlineRepository
{
    private readonly FlightDbContext _context;

    public AirlineRepository(FlightDbContext context)
    {
        _context = context;
    }

    public async Task<Airline?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Airlines.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<List<Airline>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Airlines.Where(a => !a.IsDeleted).ToListAsync(cancellationToken);
    }

    public async Task<Guid> CreateAsync(Airline airline, CancellationToken cancellationToken = default)
    {
        if (airline.Id == Guid.Empty)
            airline.Id = Guid.NewGuid();

        _context.Airlines.Add(airline);
        await _context.SaveChangesAsync(cancellationToken);
        return airline.Id;
    }

    public async Task UpdateAsync(Airline airline, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Airlines.FirstOrDefaultAsync(a => a.Id == airline.Id, cancellationToken);
        if (existing is null) return;

        existing.IataCode = airline.IataCode;
        existing.Name = airline.Name;
        existing.LogoUrl = airline.LogoUrl;
        existing.BaseCountry = airline.BaseCountry;
        existing.Address = airline.Address;
        existing.SupportEmail = airline.SupportEmail;
        existing.SupportPhone = airline.SupportPhone;
        existing.IsActive = airline.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Airlines.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (existing is null) return;

        existing.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
