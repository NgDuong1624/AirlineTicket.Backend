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

    public async Task<List<FlightDto>> SearchAsync(string origin, string destination, DateTime date, CancellationToken cancellationToken = default)
    {
        return await _context.Flights
            .Include(f => f.Route)
                .ThenInclude(r => r.OriginAirport)
            .Include(f => f.Route)
                .ThenInclude(r => r.DestinationAirport)
            .Where(f => f.Route.OriginAirport.IataCode == origin 
                     && f.Route.DestinationAirport.IataCode == destination
                     && f.ScheduledDeparture.Date == date.Date)
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
            ScheduledDeparture = DateTime.UtcNow.AddDays(1), // Mock times for now since Dto doesn't have it
            ScheduledArrival = DateTime.UtcNow.AddDays(1).AddHours(2),
            Status = AirlineTicket.Modules.Flights.Domain.Enums.FlightStatus.Scheduled
        };
        
        _context.Flights.Add(flight);
        await _context.SaveChangesAsync(cancellationToken);
        return flight.Id;
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
            query = query.Where(a => a.Name.ToLower().Contains(search) 
                                  || a.IataCode.ToLower().Contains(search) 
                                  || a.City.ToLower().Contains(search));
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
