using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;
using AirlineTicket.Modules.Flights.Domain.Enums;

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
        return await _context.Flights
            .AsNoTracking()
            .Where(f => f.Id == id && !f.IsDeleted && !f.Route.IsDeleted)
            .Select(f => new FlightDto
            {
                Id = f.Id,
                RouteId = f.RouteId,
                AirplaneId = f.AirplaneId,
                FlightNumber = f.FlightNumber,
                BasePrice = f.BasePrice,
                DepartureTime = f.DepartureTime,
                ArrivalTime = f.ArrivalTime,
                Currency = f.Currency,
                Status = (int)f.Status
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<(List<FlightDto> Items, int TotalCount)> SearchAsync(
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
        int pageIndex = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var startDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        var endDate = startDate.AddDays(1);

        var query = _context.Flights
            .AsNoTracking()
            .Where(f => !f.IsDeleted && !f.Route.IsDeleted &&
                        f.Route.OriginAirport.IataCode == origin &&
                        f.Route.DestinationAirport.IataCode == destination &&
                        f.DepartureTime >= startDate && f.DepartureTime < endDate);

        if (airlines != null && airlines.Any())
        {
            query = query.Where(f => airlines.Contains(f.Route.Airline.Name));
        }

        if (priceRangeMin.HasValue)
        {
            query = query.Where(f => f.BasePrice >= priceRangeMin.Value);
        }

        if (priceRangeMax.HasValue)
        {
            query = query.Where(f => f.BasePrice <= priceRangeMax.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var orderedQuery = sortBy?.ToLower() switch
        {
            "price_asc" => query.OrderBy(f => f.BasePrice),
            "price_desc" => query.OrderByDescending(f => f.BasePrice),
            _ => query.OrderBy(f => f.DepartureTime)
        };

        var items = await orderedQuery
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new FlightDto
            {
                Id = f.Id,
                RouteId = f.RouteId,
                AirplaneId = f.AirplaneId,
                FlightNumber = f.FlightNumber,
                BasePrice = f.BasePrice,
                DepartureTime = f.DepartureTime,
                ArrivalTime = f.ArrivalTime,
                Currency = f.Currency,
                Status = (int)f.Status,
                OriginCode = f.Route.OriginAirport.IataCode,
                DestinationCode = f.Route.DestinationAirport.IataCode,
                AirlineName = f.Route.Airline.Name
            })
            .ToListAsync(cancellationToken);

        return (items, totalCount);
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
            DepartureTime = flightDto.DepartureTime,
            ArrivalTime = flightDto.ArrivalTime,
            Status = FlightStatus.Scheduled
        };

        _context.Flights.Add(flight);

        // Seed per-flight seats from the airplane's seat template so the seat map is
        // immediately usable. No template seats → none seeded (seat map shows empty state).
        var templateSeats = await _context.AirplaneSeats
            .AsNoTracking()
            .Where(s => s.AirplaneId == flight.AirplaneId)
            .ToListAsync(cancellationToken);

        foreach (var ts in templateSeats)
        {
            var multiplier = ts.PriceMultiplier <= 0 ? 1.0m : ts.PriceMultiplier;
            _context.FlightSeats.Add(new FlightSeat
            {
                Id = Guid.NewGuid(),
                FlightId = flight.Id,
                SeatNumber = ts.SeatNumber,
                SeatClass = ts.SeatClass,
                PriceOverride = multiplier == 1.0m ? null : decimal.Round(flight.BasePrice * multiplier, 2),
                IsAvailable = true,
                IsExtraLegroom = ts.IsExtraLegroom
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return flight.Id;
    }

    public async Task<List<FlightDto>> GetTrendingAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Flights
            .AsNoTracking()
            .Where(f => !f.IsDeleted && !f.Route.IsDeleted)
            .OrderBy(f => f.BasePrice)
            .Take(5)
            .Select(f => new FlightDto
            {
                Id = f.Id,
                RouteId = f.RouteId,
                AirplaneId = f.AirplaneId,
                FlightNumber = f.FlightNumber,
                BasePrice = f.BasePrice,
                DepartureTime = f.DepartureTime,
                ArrivalTime = f.ArrivalTime,
                Currency = f.Currency,
                Status = (int)f.Status,
                OriginCode = f.Route.OriginAirport.IataCode,
                DestinationCode = f.Route.DestinationAirport.IataCode,
                AirlineName = f.Route.Airline.Name
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<StaffFlightListItemDto> Items, int TotalCount)> GetStaffFlightsAsync(string? search, Guid? airlineId = null, int pageIndex = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var query = _context.Flights
            .Include(f => f.FlightSeats)
            .AsNoTracking()
            .Where(f => !f.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(f => f.FlightNumber.Contains(s) || f.Route.OriginAirport.IataCode.Contains(s) || f.Route.DestinationAirport.IataCode.Contains(s));
        }

        if (airlineId.HasValue)
        {
            query = query.Where(f => f.Route.AirlineId == airlineId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(f => f.DepartureTime)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new StaffFlightListItemDto
            {
                Id = f.Id,
                FlightNumber = f.FlightNumber,
                DepartureTime = f.DepartureTime,
                ArrivalTime = f.ArrivalTime,
                BasePrice = f.BasePrice,
                Currency = f.Currency,
                Status = f.Status.ToString(),
                OriginCode = f.Route.OriginAirport.IataCode,
                DestinationCode = f.Route.DestinationAirport.IataCode,
                TotalSeats = f.FlightSeats.Count,
                AvailableSeats = f.FlightSeats.Count(fs => fs.IsAvailable)
            })
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(List<FlightDto> Items, int TotalCount)> GetByAirlineAsync(
        Guid airlineId,
        int pageIndex,
        int pageSize,
        string? search = null,
        int? status = null,
        DateTime? departureDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Flights
            .AsNoTracking()
            .Where(f => f.Route.AirlineId == airlineId && !f.IsDeleted && !f.Route.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(f => f.FlightNumber.ToLower().Contains(searchLower) ||
                                     f.Route.OriginAirport.IataCode.ToLower().Contains(searchLower) ||
                                     f.Route.DestinationAirport.IataCode.ToLower().Contains(searchLower));
        }

        if (status.HasValue)
        {
            query = query.Where(f => (int)f.Status == status.Value);
        }

        if (departureDate.HasValue)
        {
            var startDate = DateTime.SpecifyKind(departureDate.Value.Date, DateTimeKind.Utc);
            var endDate = startDate.AddDays(1);
            query = query.Where(f => f.DepartureTime >= startDate && f.DepartureTime < endDate);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(f => f.DepartureTime)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new FlightDto
            {
                Id = f.Id,
                RouteId = f.RouteId,
                AirplaneId = f.AirplaneId,
                FlightNumber = f.FlightNumber,
                BasePrice = f.BasePrice,
                DepartureTime = f.DepartureTime,
                ArrivalTime = f.ArrivalTime,
                Currency = f.Currency,
                Status = (int)f.Status,
                OriginCode = f.Route.OriginAirport.IataCode,
                DestinationCode = f.Route.DestinationAirport.IataCode,
                AirlineName = f.Route.Airline.Name
            })
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(List<FlightDto> Items, int TotalCount)> GetAllAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Flights
            .AsNoTracking()
            .Where(f => !f.IsDeleted && !f.Route.IsDeleted);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(f => f.DepartureTime)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new FlightDto
            {
                Id = f.Id,
                RouteId = f.RouteId,
                AirplaneId = f.AirplaneId,
                FlightNumber = f.FlightNumber,
                BasePrice = f.BasePrice,
                DepartureTime = f.DepartureTime,
                ArrivalTime = f.ArrivalTime,
                Currency = f.Currency,
                Status = (int)f.Status,
                OriginCode = f.Route.OriginAirport.IataCode,
                DestinationCode = f.Route.DestinationAirport.IataCode,
                AirlineName = f.Route.Airline.Name
            })
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<bool> UpdateAsync(FlightDto flightDto, Guid? airlineId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Flights.Include(f => f.Route).AsQueryable();
        if (airlineId.HasValue)
        {
            query = query.Where(f => f.Route.AirlineId == airlineId.Value);
        }
        var flight = await query.FirstOrDefaultAsync(f => f.Id == flightDto.Id && !f.IsDeleted, cancellationToken);

        if (flight is null) return false;

        // Only update allowed fields (DepartureTime, ArrivalTime, Status)
        flight.DepartureTime = flightDto.DepartureTime;
        flight.ArrivalTime = flightDto.ArrivalTime;

        if (flightDto.Status != 0)
        {
            flight.Status = (FlightStatus)flightDto.Status;
        }

        flight.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid? airlineId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Flights.Include(f => f.Route).AsQueryable();
        if (airlineId.HasValue)
        {
            query = query.Where(f => f.Route.AirlineId == airlineId.Value);
        }
        var flight = await query.FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted, cancellationToken);

        if (flight is null) return false;

        flight.IsDeleted = true;
        flight.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Database.CanConnectAsync(cancellationToken);
    }

    public async Task<List<FlightDto>> GetByIdsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids == null || !ids.Any()) return new List<FlightDto>();

        return await _context.Flights
            .AsNoTracking()
            .Where(f => ids.Contains(f.Id) && !f.IsDeleted && !f.Route.IsDeleted)
            .Select(f => new FlightDto
            {
                Id = f.Id,
                RouteId = f.RouteId,
                AirplaneId = f.AirplaneId,
                FlightNumber = f.FlightNumber,
                BasePrice = f.BasePrice,
                DepartureTime = f.DepartureTime,
                ArrivalTime = f.ArrivalTime,
                Currency = f.Currency,
                Status = (int)f.Status,
                OriginCode = f.Route.OriginAirport.IataCode,
                DestinationCode = f.Route.DestinationAirport.IataCode,
                AirlineName = f.Route.Airline.Name
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
        return await _context.Airports
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken);
    }

    public async Task<(List<Airport> Items, int TotalCount)> GetAllAsync(int pageIndex, int pageSize, string? search = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Airports
            .AsNoTracking()
            .Where(a => !a.IsDeleted);
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(a => a.NameEn.Contains(search) || a.NameVi.Contains(search) || a.IataCode.Contains(search) || a.CityEn.Contains(search) || a.CityVi.Contains(search));
        }
        
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(a => a.NameEn)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        
        return (items, totalCount);
    }

    public async Task<Guid> CreateAsync(Airport airport, CancellationToken cancellationToken = default)
    {
        if (airport.Id == Guid.Empty)
            airport.Id = Guid.NewGuid();

        _context.Airports.Add(airport);
        await _context.SaveChangesAsync(cancellationToken);
        return airport.Id;
    }

    public async Task<bool> UpdateAsync(Airport airport, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Airports.FirstOrDefaultAsync(a => a.Id == airport.Id && !a.IsDeleted, cancellationToken);
        if (existing is null) return false;

        existing.IataCode = airport.IataCode;
        existing.NameEn = airport.NameEn;
        existing.NameVi = airport.NameVi;
        existing.CityEn = airport.CityEn;
        existing.CityVi = airport.CityVi;
        existing.CountryCode = airport.CountryCode;
        existing.Timezone = airport.Timezone;
        existing.IsActive = airport.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Airports.FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken);
        if (existing is null) return false;

        existing.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public class RouteRepository : IRouteRepository
{
    private readonly FlightDbContext _context;

    public RouteRepository(FlightDbContext context)
    {
        _context = context;
    }

    public async Task<Route?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Routes
            .Include(r => r.Airline)
            .Include(r => r.OriginAirport)
            .Include(r => r.DestinationAirport)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);
    }

    public async Task<List<Route>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Routes
            .Include(r => r.Airline)
            .Include(r => r.OriginAirport)
            .Include(r => r.DestinationAirport)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<Route> Items, int TotalCount)> GetByAirlineAsync(Guid airlineId, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Routes
            .Where(r => r.AirlineId == airlineId && !r.IsDeleted)
            .Include(r => r.OriginAirport)
            .Include(r => r.DestinationAirport);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Guid> CreateAsync(Route route, CancellationToken cancellationToken = default)
    {
        if (route.Id == Guid.Empty)
            route.Id = Guid.NewGuid();

        _context.Routes.Add(route);
        await _context.SaveChangesAsync(cancellationToken);
        return route.Id;
    }

    public async Task<bool> UpdateAsync(Route route, Guid airlineId, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Routes
            .FirstOrDefaultAsync(r => r.Id == route.Id && r.AirlineId == airlineId && !r.IsDeleted, cancellationToken);
        if (existing is null) return false;

        existing.OriginAirportId = route.OriginAirportId;
        existing.DestinationAirportId = route.DestinationAirportId;
        existing.DistanceKm = route.DistanceKm;
        existing.EstimatedDurationMinutes = route.EstimatedDurationMinutes;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid airlineId, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Routes
            .FirstOrDefaultAsync(r => r.Id == id && r.AirlineId == airlineId && !r.IsDeleted, cancellationToken);
        if (existing is null) return false;

        existing.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public class AirplaneRepository : IAirplaneRepository
{
    private readonly FlightDbContext _context;

    public AirplaneRepository(FlightDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Airplane> Items, int TotalCount)> GetByAirlineAsync(Guid airlineId, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Airplanes
            .Where(a => a.AirlineId == airlineId && !a.IsDeleted);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Guid> CreateAsync(Airplane airplane, CancellationToken cancellationToken = default)
    {
        if (airplane.Id == Guid.Empty)
            airplane.Id = Guid.NewGuid();

        _context.Airplanes.Add(airplane);
        await _context.SaveChangesAsync(cancellationToken);
        return airplane.Id;
    }

    public async Task<bool> UpdateAsync(Airplane airplane, Guid airlineId, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Airplanes
            .FirstOrDefaultAsync(a => a.Id == airplane.Id && a.AirlineId == airlineId && !a.IsDeleted, cancellationToken);
        if (existing is null) return false;

        existing.Model = airplane.Model;
        existing.RegistrationNumber = airplane.RegistrationNumber;
        existing.TotalCapacity = airplane.TotalCapacity;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid airlineId, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Airplanes
            .FirstOrDefaultAsync(a => a.Id == id && a.AirlineId == airlineId && !a.IsDeleted, cancellationToken);
        if (existing is null) return false;

        existing.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task GenerateSeatsFromTemplateAsync(Guid airplaneId, Guid aircraftModelId, CancellationToken cancellationToken = default)
    {
        var templates = await _context.AircraftModelSeatTemplates
            .Where(t => t.AircraftModelId == aircraftModelId)
            .ToListAsync(cancellationToken);

        var seats = templates.Select(t => new AirplaneSeat
        {
            Id = Guid.NewGuid(),
            AirplaneId = airplaneId,
            SeatNumber = t.SeatNumber,
            SeatRow = t.SeatRow,
            SeatColumn = t.SeatColumn,
            SeatClass = t.SeatClass,
            IsExtraLegroom = t.IsExtraLegroom,
            PriceMultiplier = t.PriceMultiplier
        }).ToList();

        _context.AirplaneSeats.AddRange(seats);
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

    public async Task<List<FlightSeat>> GetSeatsByNumbersAsync(Guid flightId, IReadOnlyCollection<string> seatNumbers, CancellationToken cancellationToken = default)
    {
        return await _context.FlightSeats
            .AsNoTracking()
            .Where(fs => fs.FlightId == flightId && seatNumbers.Contains(fs.SeatNumber))
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetFlightBasePriceAsync(Guid flightId, CancellationToken cancellationToken = default)
    {
        return await _context.Flights
            .Where(f => f.Id == flightId)
            .Select(f => (decimal?)f.BasePrice)
            .FirstOrDefaultAsync(cancellationToken) ?? 0m;
    }

    public async Task<int> ReserveSeatAsync(Guid flightId, string seatNumber, CancellationToken cancellationToken = default)
    {
        return await _context.FlightSeats
            .Where(fs => fs.FlightId == flightId && fs.SeatNumber == seatNumber && fs.IsAvailable)
            .ExecuteUpdateAsync(s => s.SetProperty(fs => fs.IsAvailable, false), cancellationToken);
    }

    public async Task<int> ReleaseSeatsAsync(Guid flightId, IReadOnlyCollection<string> seatNumbers, CancellationToken cancellationToken = default)
    {
        return await _context.FlightSeats
            .Where(fs => fs.FlightId == flightId && seatNumbers.Contains(fs.SeatNumber))
            .ExecuteUpdateAsync(s => s.SetProperty(fs => fs.IsAvailable, true), cancellationToken);
    }

    public async Task<Dictionary<Guid, string>> GetSeatClassesAsync(List<Guid> seatIds, CancellationToken cancellationToken = default)
    {
        if (seatIds == null || !seatIds.Any()) return new Dictionary<Guid, string>();

        return await _context.FlightSeats
            .AsNoTracking()
            .Where(s => seatIds.Contains(s.Id))
            .Select(s => new { s.Id, s.SeatClass })
            .ToDictionaryAsync(s => s.Id, s => s.SeatClass.ToString(), cancellationToken);
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
        return await _context.Airlines
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken);
    }

    public async Task<(List<Airline> Items, int TotalCount)> GetAllAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Airlines
            .AsNoTracking()
            .Where(a => !a.IsDeleted);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(a => a.Name)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        
        return (items, totalCount);
    }

    public async Task<Guid> CreateAsync(Airline airline, CancellationToken cancellationToken = default)
    {
        if (airline.Id == Guid.Empty)
            airline.Id = Guid.NewGuid();

        _context.Airlines.Add(airline);
        await _context.SaveChangesAsync(cancellationToken);
        return airline.Id;
    }

    public async Task<bool> UpdateAsync(Airline airline, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Airlines.FirstOrDefaultAsync(a => a.Id == airline.Id && !a.IsDeleted, cancellationToken);
        if (existing is null) return false;

        existing.IataCode = airline.IataCode;
        existing.Name = airline.Name;
        existing.LogoUrl = airline.LogoUrl;
        existing.BaseCountry = airline.BaseCountry;
        existing.Address = airline.Address;
        existing.SupportEmail = airline.SupportEmail;
        existing.SupportPhone = airline.SupportPhone;
        existing.IsActive = airline.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Airlines.FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken);
        if (existing is null) return false;

        existing.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public class AircraftModelRepository : IAircraftModelRepository
{
    private readonly FlightDbContext _context;

    public AircraftModelRepository(FlightDbContext context)
    {
        _context = context;
    }

    public async Task<List<AircraftModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AircraftModels
            .AsNoTracking()
            .Where(m => !m.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<AircraftModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AircraftModels
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted, cancellationToken);
    }

    public async Task<Guid> CreateAsync(AircraftModel model, CancellationToken cancellationToken = default)
    {
        if (model.Id == Guid.Empty)
            model.Id = Guid.NewGuid();

        _context.AircraftModels.Add(model);
        await _context.SaveChangesAsync(cancellationToken);
        return model.Id;
    }

    public async Task<bool> UpdateAsync(AircraftModel model, CancellationToken cancellationToken = default)
    {
        var existing = await _context.AircraftModels
            .Include(m => m.SeatTemplates)
            .FirstOrDefaultAsync(m => m.Id == model.Id && !m.IsDeleted, cancellationToken);
        if (existing is null) return false;

        existing.Name = model.Name;
        existing.Manufacturer = model.Manufacturer;
        existing.TotalSeats = model.TotalSeats;

        // Simple template replacement logic
        _context.AircraftModelSeatTemplates.RemoveRange(existing.SeatTemplates);
        foreach (var t in model.SeatTemplates)
        {
            t.Id = Guid.NewGuid();
            t.AircraftModelId = existing.Id;
            _context.AircraftModelSeatTemplates.Add(t);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.AircraftModels.FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted, cancellationToken);
        if (existing is null) return false;

        existing.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
