using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Flights.Domain.Entities;

namespace AirlineTicket.Modules.Flights.Application.Contracts;

public class FlightDto
{
    public Guid Id { get; set; }
    public Guid RouteId { get; set; }
    public Guid AirplaneId { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public string OriginCode { get; set; } = string.Empty;
    public string DestinationCode { get; set; } = string.Empty;
    public string AirlineName { get; set; } = string.Empty;
    public string Currency { get; set; } = "VND";
    public int Status { get; set; }
}

public class StaffFlightListItemDto
{
    public Guid Id { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public string OriginCode { get; set; } = string.Empty;
    public string DestinationCode { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public decimal BasePrice { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
}

public interface IFlightRepository
{
    Task<FlightDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<FlightDto>> SearchAsync(
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
        CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(FlightDto flight, CancellationToken cancellationToken = default);
    Task<List<FlightDto>> GetTrendingAsync(CancellationToken cancellationToken = default);
    Task<List<StaffFlightListItemDto>> GetStaffFlightsAsync(string? search, CancellationToken cancellationToken = default);
    Task<List<FlightDto>> GetByAirlineAsync(Guid airlineId, CancellationToken cancellationToken = default);
    Task<List<FlightDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(FlightDto flight, Guid? airlineId = null, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid? airlineId = null, CancellationToken cancellationToken = default);
}

public interface IAirlineRepository
{
    Task<Airline?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(List<Airline> Items, int TotalCount)> GetAllAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(Airline airline, CancellationToken cancellationToken = default);
    Task UpdateAsync(Airline airline, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IAirportRepository
{
    Task<Airport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(List<Airport> Items, int TotalCount)> GetAllAsync(int pageIndex, int pageSize, string? search = null, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(Airport airport, CancellationToken cancellationToken = default);
    Task UpdateAsync(Airport airport, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IRouteRepository
{
    Task<List<Route>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Route>> GetByAirlineAsync(Guid airlineId, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(Route route, CancellationToken cancellationToken = default);
    Task UpdateAsync(Route route, Guid airlineId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid airlineId, CancellationToken cancellationToken = default);
}

public interface IAirplaneRepository
{
    Task<List<Airplane>> GetByAirlineAsync(Guid airlineId, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(Airplane airplane, CancellationToken cancellationToken = default);
    Task UpdateAsync(Airplane airplane, Guid airlineId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid airlineId, CancellationToken cancellationToken = default);
}

public interface IAircraftModelRepository
{
    Task<List<AircraftModel>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AircraftModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(AircraftModel model, CancellationToken cancellationToken = default);
    Task UpdateAsync(AircraftModel model, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IFlightSeatRepository
{
    Task<List<FlightSeat>> GetByFlightIdAsync(Guid flightId, CancellationToken cancellationToken = default);
}