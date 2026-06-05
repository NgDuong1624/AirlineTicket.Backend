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
}

public interface IFlightRepository
{
    Task<FlightDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<FlightDto>> SearchAsync(string origin, string destination, DateTime date, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(FlightDto flight, CancellationToken cancellationToken = default);
}

public interface IAirportRepository
{
    Task<Airport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Airport>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Airport>> SearchAsync(string? search, CancellationToken cancellationToken = default);
}

public interface IRouteRepository
{
    Task<List<Route>> GetAllAsync(CancellationToken cancellationToken = default);
}

public interface IFlightSeatRepository
{
    Task<List<FlightSeat>> GetByFlightIdAsync(Guid flightId, CancellationToken cancellationToken = default);
}
