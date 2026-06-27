using System;
using System.Linq;
using System.Threading.Tasks;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;
using AirlineTicket.Modules.Flights.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AirlineTicket.Modules.Flights.Infrastructure.Services;

public class SeatGenerationService : ISeatGenerationService
{
    private readonly FlightDbContext _context;

    public SeatGenerationService(FlightDbContext context)
    {
        _context = context;
    }

    public async Task GenerateSeatsAsync(Guid airplaneId, Guid aircraftModelId)
    {
        var templates = await _context.AircraftModelSeatTemplates
            .Where(t => t.AircraftModelId == aircraftModelId)
            .ToListAsync();

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
        await _context.SaveChangesAsync();
    }
}
