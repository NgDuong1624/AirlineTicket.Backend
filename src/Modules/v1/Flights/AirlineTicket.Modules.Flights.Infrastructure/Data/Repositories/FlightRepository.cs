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
        var connection = _context.Database.GetDbConnection();
        const string sql = @"
            SELECT f.id, f.route_id, f.airplane_id, f.flight_number, f.base_price, f.departure_time, f.arrival_time, f.currency, f.status,
                   r.airline_id
            FROM flights.flights f
            JOIN flights.routes r ON f.route_id = r.id
            WHERE f.id = @Id AND f.is_deleted = FALSE AND r.is_deleted = FALSE";
        
        return await connection.QueryFirstOrDefaultAsync<FlightDto>(sql, new { Id = id });
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
        var connection = _context.Database.GetDbConnection();
        
        var baseSql = @"
            FROM flights.flights f
            JOIN flights.routes r ON f.route_id = r.id
            JOIN flights.airports oa ON r.origin_airport_id = oa.id
            JOIN flights.airports da ON r.destination_airport_id = da.id
            JOIN flights.airlines a ON r.airline_id = a.id
            WHERE oa.iata_code = @Origin 
              AND da.iata_code = @Destination 
              AND f.departure_time::date = @Date::date
              AND f.is_deleted = FALSE AND r.is_deleted = FALSE";

        var parameters = new DynamicParameters();
        parameters.Add("Origin", origin);
        parameters.Add("Destination", destination);
        parameters.Add("Date", date.Date);

        if (airlines != null && airlines.Any())
        {
            baseSql += " AND a.name IN @Airlines";
            parameters.Add("Airlines", airlines);
        }

        if (priceRangeMin.HasValue)
        {
            baseSql += " AND f.base_price >= @MinPrice";
            parameters.Add("MinPrice", priceRangeMin.Value);
        }
        
        if (priceRangeMax.HasValue)
        {
            baseSql += " AND f.base_price <= @MaxPrice";
            parameters.Add("MaxPrice", priceRangeMax.Value);
        }

        var countSql = "SELECT COUNT(*) " + baseSql;
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

        var selectSql = $@"
            SELECT f.id, f.route_id, f.airplane_id, f.flight_number, f.base_price, 
                   f.departure_time, f.arrival_time, f.currency, f.status,
                   oa.iata_code as OriginCode, da.iata_code as DestinationCode, a.name as AirlineName
            {baseSql}
            {(string.IsNullOrEmpty(sortBy) 
                ? " ORDER BY f.departure_time OFFSET @Offset LIMIT @PageSize" 
                : (sortBy.Equals("price_asc", StringComparison.OrdinalIgnoreCase) 
                    ? " ORDER BY f.base_price ASC" 
                    : (sortBy.Equals("price_desc", StringComparison.OrdinalIgnoreCase) 
                        ? " ORDER BY f.BasePrice DESC" 
                        : "")))}";
        parameters.Add("Offset", (pageIndex - 1) * pageSize);
        parameters.Add("PageSize", pageSize);

        var result = await connection.QueryAsync<FlightDto>(selectSql, parameters);
        return (result.ToList(), totalCount);
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
            SELECT f.Id, f.RouteId, f.AirplaneId, f.FlightNumber, f.BasePrice, 
                         f.DepartureTime, f.ArrivalTime, f.Currency, f.Status,
                         oa.IataCode as OriginCode, da.IataCode as DestinationCode, a.Name as AirlineName
            FROM flights.Flights f
            JOIN flights.Routes r ON f.RouteId = r.Id
            JOIN flights.Airports oa ON r.OriginAirportId = oa.Id
            JOIN flights.Airports da ON r.DestinationAirportId = da.Id
            JOIN flights.Airlines a ON r.AirlineId = a.Id
            WHERE f.IsDeleted = FALSE AND r.IsDeleted = FALSE
            ORDER BY f.BasePrice ASC
            LIMIT 5";
        
        var result = await connection.QueryAsync<FlightDto>(sql);
        return result.ToList();
    }

    public async Task<(List<StaffFlightListItemDto> Items, int TotalCount)> GetStaffFlightsAsync(string? search, Guid? airlineId = null, int pageIndex = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        var baseSql = @"
            FROM flights.Flights f
            JOIN flights.Routes r ON f.RouteId = r.Id
            JOIN flights.Airports oa ON r.OriginAirportId = oa.Id
            JOIN flights.Airports da ON r.DestinationAirportId = da.Id
            WHERE f.IsDeleted = FALSE";

        var parameters = new DynamicParameters();
        if (!string.IsNullOrWhiteSpace(search))
        {
            baseSql += " AND (f.FlightNumber LIKE @Search OR oa.IataCode LIKE @Search OR da.IataCode LIKE @Search)";
            parameters.Add("Search", $"%{search.Trim()}%");
        }

        if (airlineId.HasValue)
        {
            baseSql += " AND r.AirlineId = @AirlineId";
            parameters.Add("AirlineId", airlineId.Value);
        }

        var countSql = "SELECT COUNT(*) " + baseSql;
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

        var sql = @"
            SELECT f.Id, f.FlightNumber, f.DepartureTime, f.ArrivalTime, f.BasePrice, f.Currency, f.Status,
                   oa.IataCode as OriginCode, da.IataCode as DestinationCode,
                   (SELECT COUNT(*) FROM flights.FlightSeats WHERE FlightId = f.Id) as TotalSeats,
                   (SELECT COUNT(*) FROM flights.FlightSeats WHERE FlightId = f.Id AND IsAvailable = TRUE) as AvailableSeats "
            + baseSql
            + " ORDER BY f.DepartureTime DESC OFFSET @Offset LIMIT @PageSize";

        parameters.Add("Offset", (pageIndex - 1) * pageSize);
        parameters.Add("PageSize", pageSize);

        var result = await connection.QueryAsync<StaffFlightListItemDto>(sql, parameters);
        return (result.ToList(), totalCount);
    }

    public async Task<(List<FlightDto> Items, int TotalCount)> GetByAirlineAsync(Guid airlineId, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        const string sql = @"
            SELECT f.Id, f.RouteId, f.AirplaneId, f.FlightNumber, f.BasePrice,
                   f.DepartureTime, f.ArrivalTime, f.Currency, f.Status,
                   oa.IataCode as OriginCode, da.IataCode as DestinationCode, a.Name as AirlineName
            FROM flights.Flights f
            JOIN flights.Routes r ON f.RouteId = r.Id
            JOIN flights.Airports oa ON r.OriginAirportId = oa.Id
            JOIN flights.Airports da ON r.DestinationAirportId = da.Id
            JOIN flights.Airlines a ON r.AirlineId = a.Id
            WHERE r.AirlineId = @AirlineId AND f.IsDeleted = FALSE AND r.IsDeleted = FALSE
            ORDER BY f.DepartureTime DESC
            OFFSET @Offset LIMIT @PageSize";

        const string countSql = @"
            SELECT COUNT(*)
            FROM flights.Flights f
            JOIN flights.Routes r ON f.RouteId = r.Id
            WHERE r.AirlineId = @AirlineId AND f.IsDeleted = FALSE AND r.IsDeleted = FALSE";

        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, new { AirlineId = airlineId });
        var result = await connection.QueryAsync<FlightDto>(sql, new { AirlineId = airlineId, Offset = (pageIndex - 1) * pageSize, PageSize = pageSize });
        return (result.ToList(), totalCount);
    }

    public async Task<(List<FlightDto> Items, int TotalCount)> GetAllAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        const string sql = @"
            SELECT f.Id, f.RouteId, f.AirplaneId, f.FlightNumber, f.BasePrice,
                   f.DepartureTime, f.ArrivalTime, f.Currency, f.Status,
                   oa.IataCode as OriginCode, da.IataCode as DestinationCode, a.Name as AirlineName
            FROM flights.Flights f
            JOIN flights.Routes r ON f.RouteId = r.Id
            JOIN flights.Airports oa ON r.OriginAirportId = oa.Id
            JOIN flights.Airports da ON r.DestinationAirportId = da.Id
            JOIN flights.Airlines a ON r.AirlineId = a.Id
            WHERE f.IsDeleted = FALSE AND r.IsDeleted = FALSE
            ORDER BY f.DepartureTime DESC
            OFFSET @Offset LIMIT @PageSize";

        const string countSql = @"
            SELECT COUNT(*)
            FROM flights.Flights f
            JOIN flights.Routes r ON f.RouteId = r.Id
            WHERE f.IsDeleted = FALSE AND r.IsDeleted = FALSE";

        var totalCount = await connection.ExecuteScalarAsync<int>(countSql);
        var result = await connection.QueryAsync<FlightDto>(sql, new { Offset = (pageIndex - 1) * pageSize, PageSize = pageSize });
        return (result.ToList(), totalCount);
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

        // Only update allowed fields (DepartureTime, ArrivalTime, Status)
        flight.DepartureTime = flightDto.DepartureTime;
        flight.ArrivalTime = flightDto.ArrivalTime;

        if (flightDto.Status != 0)
        {
            flight.Status = (FlightStatus)flightDto.Status;
        }

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

    public async Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Database.CanConnectAsync(cancellationToken);
    }

    public async Task<List<FlightDto>> GetByIdsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids == null || !ids.Any()) return new List<FlightDto>();

        var connection = _context.Database.GetDbConnection();
        const string sql = @"
            SELECT f.Id, f.RouteId, f.AirplaneId, f.FlightNumber, f.BasePrice,
                   f.DepartureTime, f.ArrivalTime, f.Currency, f.Status,
                   oa.IataCode as OriginCode, da.IataCode as DestinationCode, a.Name as AirlineName
            FROM flights.Flights f
            JOIN flights.Routes r ON f.RouteId = r.Id
            JOIN flights.Airports oa ON r.OriginAirportId = oa.Id
            JOIN flights.Airports da ON r.DestinationAirportId = da.Id
            JOIN flights.Airlines a ON r.AirlineId = a.Id
            WHERE f.Id = ANY(@Ids) AND f.IsDeleted = FALSE AND r.IsDeleted = FALSE";

        var result = await connection.QueryAsync<FlightDto>(sql, new { Ids = ids });
        return result.ToList();
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
            "SELECT * FROM flights.Airports WHERE Id = @Id AND IsDeleted = FALSE", new { Id = id });
    }

    public async Task<(List<Airport> Items, int TotalCount)> GetAllAsync(int pageIndex, int pageSize, string? search = null, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        string sql = "SELECT * FROM flights.Airports WHERE IsDeleted = FALSE";
        string countSql = "SELECT COUNT(*) FROM flights.Airports WHERE IsDeleted = FALSE";
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            sql += " AND (NameEn LIKE @Search OR NameVi LIKE @Search OR IataCode LIKE @Search OR CityEn LIKE @Search OR CityVi LIKE @Search)";
            countSql += " AND (NameEn LIKE @Search OR NameVi LIKE @Search OR IataCode LIKE @Search OR CityEn LIKE @Search OR CityVi LIKE @Search)";
            search = $"%{search}%";
        }
        
        sql += " ORDER BY NameEn OFFSET @Offset LIMIT @PageSize";
        
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

    public async Task<Route?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Routes
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
        var connection = _context.Database.GetDbConnection();
        return await connection.QueryFirstOrDefaultAsync<Airline>(
            "SELECT * FROM flights.Airlines WHERE id = @Id AND is_deleted = 0", new { Id = id });
    }

    public async Task<(List<Airline> Items, int TotalCount)> GetAllAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        var items = (await connection.QueryAsync<Airline>(
            "SELECT * FROM flights.Airlines WHERE is_deleted = 0 ORDER BY name OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY",
            new { Offset = (pageIndex - 1) * pageSize, PageSize = pageSize })).ToList();
        var totalCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM flights.Airlines WHERE is_deleted = 0");
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
        return (await connection.QueryAsync<AircraftModel>("SELECT * FROM flights.AircraftModels WHERE is_deleted = 0")).ToList();
    }

    public async Task<AircraftModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        return await connection.QueryFirstOrDefaultAsync<AircraftModel>(
            "SELECT * FROM flights.AircraftModels WHERE id = @Id AND is_deleted = 0", new { Id = id });
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
