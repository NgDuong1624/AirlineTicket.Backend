using Dapper;
using System.Data;
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
        var connection = _context.Database.GetDbConnection();
        const string sql = @"
            SELECT f.Id, f.DepartureTime, f.ArrivalTime, f.BasePrice as Price, f.Status,
                   r.OriginAirportId, r.DestinationAirportId, r.AirlineId
            FROM dbo.Flights f
            JOIN dbo.Routes r ON f.RouteId = r.Id
            WHERE f.Id = @Id AND f.IsDeleted = 0 AND r.IsDeleted = 0";
        
        return await connection.QueryFirstOrDefaultAsync<FlightDto>(sql, new { Id = id });
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
        var connection = _context.Database.GetDbConnection();
        
        var sql = @"
            SELECT f.Id, f.RouteId, f.AirplaneId, f.FlightNumber, f.BasePrice, 
                   f.DepartureTime, f.ArrivalTime, f.Currency, f.Status,
                   oa.IataCode as OriginCode, da.IataCode as DestinationCode, a.Name as AirlineName
            FROM dbo.Flights f
            JOIN dbo.Routes r ON f.RouteId = r.Id
            JOIN dbo.Airports oa ON r.OriginAirportId = oa.Id
            JOIN dbo.Airports da ON r.DestinationAirportId = da.Id
            JOIN dbo.Airlines a ON r.AirlineId = a.Id
            WHERE oa.IataCode = @Origin 
              AND da.IataCode = @Destination 
              AND CAST(f.DepartureTime AS DATE) = CAST(@Date AS DATE)
              AND f.IsDeleted = 0 AND r.IsDeleted = 0";

        var parameters = new DynamicParameters();
        parameters.Add("Origin", origin);
        parameters.Add("Destination", destination);
        parameters.Add("Date", date.Date);

        if (airlines != null && airlines.Any())
        {
            sql += " AND a.Name IN @Airlines";
            parameters.Add("Airlines", airlines);
        }

        if (priceRangeMin.HasValue)
        {
            sql += " AND f.BasePrice >= @MinPrice";
            parameters.Add("MinPrice", priceRangeMin.Value);
        }
        
        if (priceRangeMax.HasValue)
        {
            sql += " AND f.BasePrice <= @MaxPrice";
            parameters.Add("MaxPrice", priceRangeMax.Value);
        }

        if (!string.IsNullOrEmpty(sortBy))
        {
            if (sortBy.Equals("price_asc", StringComparison.OrdinalIgnoreCase))
                sql += " ORDER BY f.BasePrice ASC";
            else if (sortBy.Equals("price_desc", StringComparison.OrdinalIgnoreCase))
                sql += " ORDER BY f.BasePrice DESC";
        }

        var result = await connection.QueryAsync<FlightDto>(sql, parameters);
        return result.ToList();
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
        var connection = _context.Database.GetDbConnection();
        const string sql = @"
            SELECT TOP 5 f.Id, f.RouteId, f.AirplaneId, f.FlightNumber, f.BasePrice, 
                         f.DepartureTime, f.ArrivalTime, f.Currency, f.Status,
                         oa.IataCode as OriginCode, da.IataCode as DestinationCode, a.Name as AirlineName
            FROM dbo.Flights f
            JOIN dbo.Routes r ON f.RouteId = r.Id
            JOIN dbo.Airports oa ON r.OriginAirportId = oa.Id
            JOIN dbo.Airports da ON r.DestinationAirportId = da.Id
            JOIN dbo.Airlines a ON r.AirlineId = a.Id
            WHERE f.IsDeleted = 0 AND r.IsDeleted = 0
            ORDER BY f.BasePrice ASC";
        
        var result = await connection.QueryAsync<FlightDto>(sql);
        return result.ToList();
    }

    public async Task<List<StaffFlightListItemDto>> GetStaffFlightsAsync(string? search, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        var sql = @"
            SELECT f.Id, f.FlightNumber, f.DepartureTime, f.ArrivalTime, f.BasePrice, f.Currency, f.Status,
                   oa.IataCode as OriginCode, da.IataCode as DestinationCode,
                   (SELECT COUNT(*) FROM dbo.FlightSeats WHERE FlightId = f.Id) as TotalSeats,
                   (SELECT COUNT(*) FROM dbo.FlightSeats WHERE FlightId = f.Id AND IsAvailable = 1) as AvailableSeats
            FROM dbo.Flights f
            JOIN dbo.Routes r ON f.RouteId = r.Id
            JOIN dbo.Airports oa ON r.OriginAirportId = oa.Id
            JOIN dbo.Airports da ON r.DestinationAirportId = da.Id
            WHERE f.IsDeleted = 0";

        var parameters = new DynamicParameters();
        if (!string.IsNullOrWhiteSpace(search))
        {
            sql += " AND (f.FlightNumber LIKE @Search OR oa.IataCode LIKE @Search OR da.IataCode LIKE @Search)";
            parameters.Add("Search", $"%{search.Trim()}%");
        }

        sql += " ORDER BY f.DepartureTime";

        var result = await connection.QueryAsync<StaffFlightListItemDto>(sql, parameters);
        return result.ToList();
    }

    public async Task<List<FlightDto>> GetByAirlineAsync(Guid airlineId, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        const string sql = @"
            SELECT f.Id, f.RouteId, f.AirplaneId, f.FlightNumber, f.BasePrice,
                   f.DepartureTime, f.ArrivalTime, f.Currency, f.Status,
                   oa.IataCode as OriginCode, da.IataCode as DestinationCode, a.Name as AirlineName
            FROM dbo.Flights f
            JOIN dbo.Routes r ON f.RouteId = r.Id
            JOIN dbo.Airports oa ON r.OriginAirportId = oa.Id
            JOIN dbo.Airports da ON r.DestinationAirportId = da.Id
            JOIN dbo.Airlines a ON r.AirlineId = a.Id
            WHERE r.AirlineId = @AirlineId AND f.IsDeleted = 0 AND r.IsDeleted = 0";

        var result = await connection.QueryAsync<FlightDto>(sql, new { AirlineId = airlineId });
        return result.ToList();
    }

    public async Task<List<FlightDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        const string sql = @"
            SELECT f.Id, f.RouteId, f.AirplaneId, f.FlightNumber, f.BasePrice,
                   f.DepartureTime, f.ArrivalTime, f.Currency, f.Status,
                   oa.IataCode as OriginCode, da.IataCode as DestinationCode, a.Name as AirlineName
            FROM dbo.Flights f
            JOIN dbo.Routes r ON f.RouteId = r.Id
            JOIN dbo.Airports oa ON r.OriginAirportId = oa.Id
            JOIN dbo.Airports da ON r.DestinationAirportId = da.Id
            JOIN dbo.Airlines a ON r.AirlineId = a.Id
            WHERE f.IsDeleted = 0 AND r.IsDeleted = 0";

        var result = await connection.QueryAsync<FlightDto>(sql);
        return result.ToList();
    }

    public async Task UpdateAsync(FlightDto flightDto, Guid? airlineId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Flights.Include(f => f.Route).AsQueryable();
        if (airlineId.HasValue)
        {
            query = query.Where(f => f.Route.AirlineId == airlineId.Value);
        }
        var flight = await query.FirstOrDefaultAsync(f => f.Id == flightDto.Id && !f.IsDeleted, cancellationToken);

        if (flight is null) return;

        flight.FlightNumber = flightDto.FlightNumber;
        flight.BasePrice = flightDto.BasePrice;
        flight.DepartureTime = flightDto.DepartureTime;
        flight.ArrivalTime = flightDto.ArrivalTime;
        flight.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, Guid? airlineId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Flights.Include(f => f.Route).AsQueryable();
        if (airlineId.HasValue)
        {
            query = query.Where(f => f.Route.AirlineId == airlineId.Value);
        }
        var flight = await query.FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted, cancellationToken);

        if (flight is null) return;

        flight.IsDeleted = true;
        flight.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
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
        var connection = _context.Database.GetDbConnection();
        return await connection.QueryFirstOrDefaultAsync<Airport>(
            "SELECT * FROM dbo.Airports WHERE Id = @Id AND IsDeleted = 0", new { Id = id });
    }

    public async Task<(List<Airport> Items, int TotalCount)> GetAllAsync(int pageIndex, int pageSize, string? search = null, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        string sql = "SELECT * FROM dbo.Airports WHERE IsDeleted = 0";
        string countSql = "SELECT COUNT(*) FROM dbo.Airports WHERE IsDeleted = 0";
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            sql += " AND (NameEn LIKE @Search OR NameVi LIKE @Search OR IataCode LIKE @Search OR CityEn LIKE @Search OR CityVi LIKE @Search)";
            countSql += " AND (NameEn LIKE @Search OR NameVi LIKE @Search OR IataCode LIKE @Search OR CityEn LIKE @Search OR CityVi LIKE @Search)";
            search = $"%{search}%";
        }
        
        sql += " ORDER BY NameEn OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
        
        var items = (await connection.QueryAsync<Airport>(sql, new { Search = search, Offset = (pageIndex - 1) * pageSize, PageSize = pageSize })).ToList();
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, new { Search = search });
        
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
        var connection = _context.Database.GetDbConnection();
        return await connection.QueryFirstOrDefaultAsync<Airline>(
            "SELECT * FROM dbo.Airlines WHERE Id = @Id AND IsDeleted = 0", new { Id = id });
    }

    public async Task<(List<Airline> Items, int TotalCount)> GetAllAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        var items = (await connection.QueryAsync<Airline>(
            "SELECT * FROM dbo.Airlines WHERE IsDeleted = 0 ORDER BY Name OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY",
            new { Offset = (pageIndex - 1) * pageSize, PageSize = pageSize })).ToList();
        var totalCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM dbo.Airlines WHERE IsDeleted = 0");
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

public class AircraftModelRepository : IAircraftModelRepository
{
    private readonly FlightDbContext _context;

    public AircraftModelRepository(FlightDbContext context)
    {
        _context = context;
    }

    public async Task<List<AircraftModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        return (await connection.QueryAsync<AircraftModel>("SELECT * FROM dbo.AircraftModels WHERE IsDeleted = 0")).ToList();
    }

    public async Task<AircraftModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        return await connection.QueryFirstOrDefaultAsync<AircraftModel>(
            "SELECT * FROM dbo.AircraftModels WHERE Id = @Id AND IsDeleted = 0", new { Id = id });
    }

    public async Task<Guid> CreateAsync(AircraftModel model, CancellationToken cancellationToken = default)
    {
        if (model.Id == Guid.Empty)
            model.Id = Guid.NewGuid();

        _context.AircraftModels.Add(model);
        await _context.SaveChangesAsync(cancellationToken);
        return model.Id;
    }

    public async Task UpdateAsync(AircraftModel model, CancellationToken cancellationToken = default)
    {
        var existing = await _context.AircraftModels
            .Include(m => m.SeatTemplates)
            .FirstOrDefaultAsync(m => m.Id == model.Id && !m.IsDeleted, cancellationToken);
        if (existing is null) return;

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
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.AircraftModels.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        if (existing is null) return;

        existing.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
