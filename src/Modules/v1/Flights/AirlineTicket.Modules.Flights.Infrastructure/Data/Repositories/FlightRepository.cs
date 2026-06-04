using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Flights.Application.Contracts;

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
        // TODO: Implement when entities are properly set up
        return await Task.FromResult<FlightDto?>(null);
    }

    public async Task<List<FlightDto>> SearchAsync(string origin, string destination, DateTime date, CancellationToken cancellationToken = default)
    {
        // TODO: Implement when entities are properly set up
        return await Task.FromResult(new List<FlightDto>());
    }

    public async Task<Guid> CreateAsync(FlightDto flight, CancellationToken cancellationToken = default)
    {
        // TODO: Implement when entities are properly set up
        return await Task.FromResult(Guid.NewGuid());
    }
}

public class AirportRepository : IAirportRepository
{
    private readonly FlightDbContext _context;

    public AirportRepository(FlightDbContext context)
    {
        _context = context;
    }

    public async Task<object?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // TODO: Implement when entities are properly set up
        return await Task.FromResult<object?>(null);
    }

    public async Task<List<object>> SearchAsync(string? search, CancellationToken cancellationToken = default)
    {
        // TODO: Implement when entities are properly set up
        return await Task.FromResult(new List<object>());
    }
}
